using Nito.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Ugit
{
    internal class BaseOperator : IBaseOperator
    {
        private readonly IFileSystem fileSystem;

        private readonly IDataProvider dataProvider;

        private readonly IDiff diff;

        public BaseOperator(IFileSystem fileSystem, IDataProvider dataprovider, IDiff diff)
        {
            this.fileSystem = fileSystem;
            this.dataProvider = dataprovider;
            this.diff = diff;
        }

        public string WriteTree(string directory = ".")
        {
            List<(string, string, string)> entries = new List<(string, string, string)>();
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                if (IsIgnore(filePath)) continue;
                byte[] data = fileSystem.File.ReadAllBytes(filePath);
                string name = Path.GetRelativePath(directory, filePath);
                string oid = dataProvider.HashObject(data);
                string type = "blob";
                entries.Add((name, oid, type));
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                if (IsIgnore(directoryPath)) continue;
                string oid = WriteTree(directoryPath);
                string name = Path.GetRelativePath(directory, directoryPath);
                string type = "tree";
                entries.Add((name, oid, type));
            }
            // type oid name
            string tree = string.Join("\n", 
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return dataProvider.HashObject(tree.Encode(), "tree");
        }

        private IEnumerable<(string, string, string)> IterTreeEntry(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
                yield break;
            byte[] tree = dataProvider.GetObject(oid, "tree");
            foreach (string entry in tree.Decode().Split("\n"))
            {
                string[] tokens = entry.Split(' ');
                if(tokens.Length >= 3)
                {
                    yield return (tokens[0], tokens[1], tokens[2]);
                }
            }
        }

        public IDictionary<string, string> GetTree(string treeOid, string basePath = "")
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (type, oid, name) in IterTreeEntry(treeOid))
            {
                string path = Path.Join(basePath, name);
                if (type == "blob")
                {
                    result[path] = oid;
                }
                else if (type == "tree")
                {
                    result.Update(GetTree(oid, $"{path}{Path.DirectorySeparatorChar}"));
                }
                else
                {
                    throw new ArgumentException($"Unknown type {type} of object {oid}");
                }
            }

            return result;
        }

        private void EmptyCurrentDirectory()
        {
            foreach (var filePath in fileSystem.Directory.EnumerateFiles("."))
            {
                if (IsIgnore(filePath)) continue;
                fileSystem.File.Delete(filePath);
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories("."))
            {
                if (IsIgnore(directoryPath)) continue;
                fileSystem.Directory.Delete(directoryPath, true);
            }
        }

        public void ReadTree(string treeOid)
        {
            EmptyCurrentDirectory();
            foreach (var entry in GetTree(treeOid, "."))
            {
                string path = entry.Key;
                string oid = entry.Value;
                fileSystem.CreateParentDirectory(path);
                byte[] data = dataProvider.GetObject(oid);
                fileSystem.File.WriteAllBytes(path, data);
            }
        }

        private bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(dataProvider.GitDir);

        public string Commit(string message)
        {
            string commit = $"tree {WriteTree()}\n";
            string HEAD = dataProvider.GetRef("HEAD").Value;
            if(!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            string mergeHead = dataProvider.GetRef("MERGE_HEAD").Value;
            if(!string.IsNullOrEmpty(mergeHead))
            {
                commit += $"parent {mergeHead}\n";
                dataProvider.DeleteRef("MERGE_HEAD", false);
            }

            commit += "\n";
            commit += $"{message}\n";

            string oid = dataProvider.HashObject(commit.Encode(), "commit");
            dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
            return oid;
        }

        public Commit GetCommit(string oid)
        {
            var parents = new List<string>();
            var commit = dataProvider.GetObject(oid, "commit").Decode();
            string[] lines = commit.Split("\n");
            string tree=null, parent=null;
            int index;
            for (index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (string.IsNullOrEmpty(line))
                {
                    break;
                }
                string[] tokens = line.Split(' ');
                if (tokens[0].Equals("tree"))
                {
                    tree = tokens[1];
                }
                if (tokens[0].Equals("parent"))
                {
                    parents.Add(tokens[1]);
                }
            }

            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parents = parents,
                Message = message
            };
        }

        public void Checkout(string name)
        {
            string oid = GetOid(name);
            var commit = GetCommit(oid);
            ReadTree(commit.Tree);

            RefValue HEAD;
            if(IsBranch(name))
            {
                HEAD = RefValue.Create(true, Path.Join("refs", "heads", name));
            }
            else
            {
                HEAD = RefValue.Create(false, oid);
            }

            dataProvider.UpdateRef("HEAD", HEAD, false);
        }

        public void CreateTag(string name, string oid)
        {
            string @ref = Path.Join("refs", "tags", name);
            dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        public string GetOid(string name)
        {
            name = name == "@" ? "HEAD" : name;
            string[] refsToTry = new string[]
            {
                Path.Join(name),
                Path.Join("refs", name),
                Path.Join("refs", "tags", name),
                Path.Join("refs", "heads", name)
            };
            foreach (var @ref in refsToTry)
            {
                if(!string.IsNullOrEmpty(dataProvider.GetRef(@ref, false).Value))
                {
                    return dataProvider.GetRef(@ref).Value;
                }
            }

            if(name.IsOnlyHex() && name.Length == 40)
            {
                return name;
            }

            return null;
        }

        public IEnumerable<string> IterCommitsAndParents(IEnumerable<string> oids)
        {
            Deque<string> oidQueue = new Deque<string>(oids);
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while(oidQueue.Count > 0)
            {
                string oid = oidQueue.RemoveFromFront();
                if (string.IsNullOrWhiteSpace(oid) || visited.Contains(oid)) continue;
                visited.Add(oid);
                yield return oid;

                var commit = GetCommit(oid);
                oidQueue.AddToFront(commit.Parents.FirstOrDefault());
                if (commit.Parents.Count > 1)
                {
                    commit.Parents.TakeLast(commit.Parents.Count - 1)
                        .ToList()
                        .ForEach(id => oidQueue.AddToBack(id));
                }
            }
        }

        public void CreateBranch(string name, string oid)
        {
            string @ref = Path.Join("refs", "heads", name);
            dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        private bool IsBranch(string branch)
        {
            string path = Path.Join("refs", "heads", branch);
            return !string.IsNullOrWhiteSpace(dataProvider.GetRef(path).Value);
        }

        public void Init()
        {
            dataProvider.Init();
            dataProvider.UpdateRef("HEAD", RefValue.Create(true, Path.Join("refs", "heads", "master")));
        }

        public string GetBranchName()
        {
            var HEAD = dataProvider.GetRef("HEAD", false);
            if(!HEAD.Symbolic)
            {
                return null;
            }

            var head = HEAD.Value;
            Debug.Assert(head.StartsWith(Path.Join("refs", "heads")));
            return Path.GetRelativePath(Path.Join("refs", "heads"), head);
        }

        public IEnumerable<string> IterBranchNames()
        {
            foreach (var (refName, _) in dataProvider.IterRefs(Path.Join("refs", "heads")))
            {
                yield return Path.GetRelativePath(Path.Join("refs", "heads"), refName);
            }
        }

        public void Reset(string oid)
        {
            dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
        }

        public IDictionary<string, string> GetWorkingTree()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var filePath in fileSystem.Walk("."))
            {
                string path = Path.GetRelativePath(".", filePath);
                if (IsIgnore(path))
                {
                    continue;
                }

                result[path] = dataProvider.HashObject(fileSystem.File.ReadAllBytes(path));
            }

            return result;
        }

        public void Merge(string other)
        {
            string head = dataProvider.GetRef("HEAD").Value;
            var headCommit = GetCommit(head);
            var otherCommit = GetCommit(other);

            dataProvider.UpdateRef("MERGE_HEAD", RefValue.Create(false, other));
            ReadTreeMerged(headCommit.Tree, otherCommit.Tree);
            Console.WriteLine("Merged in working tree\nPlease commit");
        }

        private void ReadTreeMerged(string headOid, string otherOid)
        {
            EmptyCurrentDirectory();
            foreach (var entry in diff.MergeTree(GetTree(headOid), GetTree(otherOid)))
            {
                string path = entry.Key;
                string blob = entry.Value;
                this.fileSystem.CreateParentDirectory(path);
                fileSystem.File.WriteAllBytes(path, blob.Encode());
            }
        }

        public string GetMergeBase(string oid1, string oid2)
        {
            IEnumerable<string> parents = IterCommitsAndParents(new [] { oid1 });
            foreach (var oid in IterCommitsAndParents(new[] { oid2 }))
            {
                if (parents.Contains(oid))
                    return oid;
            }
            return null;
        }
    }
}
