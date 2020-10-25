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

        public string WriteTree()
        {
            IDictionary<string, object> indexAsTree = new Dictionary<string, object>();
            Dictionary<string, string> index = dataProvider.GetIndex();
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                string[] tokens = path.Split(Path.DirectorySeparatorChar);
                var current = indexAsTree;
                if (tokens.Length == 1)
                {
                    string fileName = tokens[0];
                    current[fileName] = oid;
                }
                else
                {
                    string fileName = tokens[^1];
                    string[] dirPath = tokens.Take(tokens.Length - 1).ToArray();
                    foreach (var dirName in dirPath)
                    {
                        if (!current.ContainsKey(dirName))
                        {
                            current[dirName] = new Dictionary<string, object>();

                        }
                        current = current[dirName] as IDictionary<string, object>;
                    }
                    current[fileName] = oid;
                }
            }
            dataProvider.SetIndex(index);
            return WriteTreeRecursive(indexAsTree);
        }

        private string WriteTreeRecursive(IDictionary<string, object> treeDict)
        {
            List<(string, string, string)> entries = new List<(string, string, string)>();
            foreach (var entry in treeDict)
            {
                if (entry.Value is IDictionary<string, object> val)
                {
                    string type = "tree";
                    string oid = WriteTreeRecursive(val);
                    string name = entry.Key;
                    entries.Add((name, oid, type));
                }
                else
                {
                    string type = "blob";
                    string oid = entry.Value as string;
                    string name = entry.Key;
                    entries.Add((name, oid, type));
                }
            }
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

        public void ReadTree(string treeOid,bool updateWorking=false)
        {
            var index = dataProvider.GetIndex();
            index.Clear();
            index.Update(GetTree(treeOid));
            if (updateWorking)
            {
                CheckoutIndex(index);
            }
            dataProvider.SetIndex(index);
        }

        private void CheckoutIndex(Dictionary<string, string> index)
        {
            EmptyCurrentDirectory();
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                fileSystem.CreateParentDirectory(path);
                fileSystem.File.WriteAllBytes(path, dataProvider.GetObject(oid, "blob"));
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
            ReadTree(commit.Tree, true);

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
            string mergeBase = GetMergeBase(other, head);
            var otherCommit = GetCommit(other);

            if (mergeBase == head)
            {
                ReadTree(otherCommit.Tree, true);
                dataProvider.UpdateRef("HEAD", RefValue.Create(false, other));
                Console.WriteLine("Fast-forwad, no need to commit");
                return;
            }

            dataProvider.UpdateRef("MERGE_HEAD", RefValue.Create(false, other));
            ReadTreeMerged(headCommit.Tree, otherCommit.Tree, true);
            Console.WriteLine("Merged in working tree\nPlease commit");
        }

        private void ReadTreeMerged(string headTree, string otherTree, bool updateWorking = false)
        {
            var index = dataProvider.GetIndex();
            index.Clear();
            index.Update(diff.MergeTree(GetTree(headTree), GetTree(otherTree)));
            if (updateWorking)
            {
                CheckoutIndex(index);
            }
            dataProvider.SetIndex(index);
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

        public void Add(IEnumerable<string> fileNames)
        {
            var index = dataProvider.GetIndex();

            foreach (var name in fileNames)
            {
                if(fileSystem.File.Exists(name))
                {
                    AddFile(index, name);
                }
                else if (fileSystem.Directory.Exists(name))
                {
                    AddDirecotry(index, name);
                }
            }

            dataProvider.SetIndex(index);
        }

        private void AddFile(IDictionary<string, string> index, string fileName)
        {
            var normalFileName = Path.GetRelativePath(".", fileName);
            byte[] data = fileSystem.File.ReadAllBytes(normalFileName);
            string oid = dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }

        private void AddDirecotry(IDictionary<string, string> index, string dirName)
        {
            foreach (var fileName in fileSystem.Walk(dirName))
            {
                if(!IsIgnore(fileName))
                {
                    AddFile(index, fileName);
                }
                    
            }
        }

        public Dictionary<string, string> GetIndexTree()
        {
            return dataProvider.GetIndex();
        }
    }
}
