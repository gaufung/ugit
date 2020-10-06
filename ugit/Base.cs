using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using Nito.Collections;

namespace ugit
{
    public class Base
    {
        private readonly Data data;

        private readonly IFileSystem fileSystem;

        private readonly Diff diff;

        public Base(Data data, IFileSystem fileSystem, Diff diff)
        {
            this.data = data;
            this.fileSystem = fileSystem;
            this.diff = diff;
        }

        public void Init()
        {
            data.Init();
            data.UpdateRef("HEAD", 
                RefValue.Create(true, fileSystem.Path.Join("refs", "heads", "master")));
        }

        public string WriteTree(string directory = ".")
        {
            List<ValueTuple<string, string, string>> entries = new List<(string, string, string)>();
            
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                if(IsIgnore(filePath)) continue;
                string @type = "blob";
                string oid = data.HashObject(fileSystem.File.ReadAllBytes(filePath));
                string name = fileSystem.Path.GetRelativePath(directory, filePath);
                entries.Add(ValueTuple.Create(name, oid, @type));
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                if(IsIgnore(directoryPath)) continue;
                string type = "tree";
                string oid = WriteTree(directoryPath);
                string name = fileSystem.Path.GetRelativePath(directory, directoryPath);
                entries.Add(ValueTuple.Create(name, oid, @type));
            }

            string tree = string.Join("\n", 
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return data.HashObject(tree.Encode(), "tree");
        }

        private IEnumerable<ValueTuple<string, string, string>> IterTreeEntries(string oid)
        {
            if(string.IsNullOrWhiteSpace(oid)) yield break;
            byte[] tree = data.GetObject(oid, "tree");
            foreach (var line in tree.Decode().Split("\n"))
            {
                string[] tokens = line.Split(' ', 3);
                yield return ValueTuple.Create(tokens[0], tokens[1], tokens[2]);
            }
        }

        public IDictionary<string, string> GetTree(string oid, string basePath="")
        {
            IDictionary<string, string> result = new Dictionary<string, string>();
            foreach (var (type, objectId, name) in IterTreeEntries(oid))
            {
                Debug.Assert(!name.Contains(fileSystem.Path.DirectorySeparatorChar));
                Debug.Assert(name != "." && name != "..");
                string path = fileSystem.Path.Join(basePath, name);
                if (type == "blob")
                {
                    result[path] = objectId;
                }
                else if (type == "tree")
                {
                    result.Update(GetTree(objectId, path));
                }
                else
                {
                    Debug.Assert(false, $"unknown tree entry {type}");
                }
            }

            return result;
        }

        public IDictionary<string, string> GetWorkingTree()
        {
            var result = new Dictionary<string, string>();
            foreach (var filePath in fileSystem.Walk("."))
            {
                string path = fileSystem.Path.GetRelativePath(".", filePath);
                if (IsIgnore(path) || !fileSystem.File.Exists(filePath))
                {
                    continue;
                }

                result[path] = data.HashObject(fileSystem.File.ReadAllBytes(path));
            }

            return result;
        }

        private void EmptyCurrentDirectory()
        {
            foreach (var filePath in fileSystem.Directory.EnumerateFiles("."))
            {
                if(IsIgnore(filePath)) continue;
                fileSystem.File.Delete(filePath);
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories("."))
            {
                if(IsIgnore(directoryPath)) continue;
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
                path.CreateParentDirectory(fileSystem);
                fileSystem.File.WriteAllBytes(path, data.GetObject(oid));
            }
        }

        public void ReadTreeMerge(string headTree, string otherTree)
        {
            EmptyCurrentDirectory();
            foreach (var entry in diff.MergeTree(GetTree(headTree), GetTree(otherTree)))
            {
                string path = entry.Key;
                string blob = entry.Value;
                path.CreateParentDirectory(fileSystem);
                fileSystem.File.WriteAllText(path, blob);
            }
        }
        public string Commit(string message)
        {
            string commit = $"tree {WriteTree()}\n";
            string HEAD = data.GetRef("HEAD").Value;
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = data.HashObject(commit.Encode(), "commit");
            data.UpdateRef("HEAD",RefValue.Create(false, oid));
            return oid;
        }

        public void CheckOut(string name)
        {
            string oid = GetOid(name);
            var commit = GetCommit(oid);
            ReadTree(commit.Tree);

            RefValue HEAD = IsBranch(name) ? 
                RefValue.Create(true, fileSystem.Path.Join("refs", "heads", name)) : 
                RefValue.Create(false, oid);
            
            data.UpdateRef("HEAD", HEAD, false);
        }

        public void Reset(string oid)
        {
            data.UpdateRef("HEAD", RefValue.Create(false,oid));
        }

        public void Merge(string other)
        {
            string head = data.GetRef("HEAD").Value;
            Commit headCommit = GetCommit(head);
            Commit otherCommit = GetCommit(other);
            ReadTreeMerge(headCommit.Tree, otherCommit.Tree);
            Console.WriteLine("Merge in working tree");
        }
        
        public void CreateTag(string name, string oid)
        {
            string @ref = fileSystem.Path.Join("refs", "tags", name);
            data.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        public void CreateBranch(string name, string oid)
        {
            string @ref = fileSystem.Path.Join("refs", "heads", name);
            data.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        public IEnumerable<string> IterBranchName()
        {
            string branchPrefix = fileSystem.Path.Join("refs", "heads");
            string head = fileSystem.Path.Join("refs", "heads");
            foreach (var (refName, _) in data.IterRefs(branchPrefix))
            {
                yield return fileSystem.Path.GetRelativePath(head, refName);
            }
        }

        private bool IsBranch(string branch)
        {
            string @ref = fileSystem.Path.Join("refs", "heads", branch);
            return !string.IsNullOrWhiteSpace(data.GetRef(@ref).Value);
        }

        public string GetBranchName()
        {
            var HEAD = data.GetRef("HEAD", false);
            if (!HEAD.Symbolic)
            {
                return null;
            }

            string head = HEAD.Value;
            Debug.Assert(head.StartsWith("refs/heads/"));
            return fileSystem.Path.GetRelativePath("refs/heads", head);
        }
        
        public Commit GetCommit(string oid)
        {
            string commit = data.GetObject(oid, "commit").Decode();
            string tree = null;
            string parent = null;
            string[] lines = commit.Split('\n');
            int count = 0;
            foreach (var line in lines.TakeWhile(l => !string.IsNullOrWhiteSpace(l)))
            {
                string[] tokens = line.Split(' ', 2);
                if (tokens[0] == "tree")
                {
                    tree = tokens[1];
                }
                else if (tokens[0] == "parent")
                {
                    parent = tokens[1];
                }
                else
                {
                    Debug.Assert(false, $"unknown field {tokens[0]}");
                }

                count++;
            }

            string message = string.Join('\n', lines.TakeLast(lines.Length - count - 1));
            return new Commit()
            {
                Tree = tree,
                Parent = parent,
                Message =  message,
            };
        }

        public IEnumerable<string> IterCommitAndParents(IEnumerable<string> oids)
        {
            Deque<string> oidsQueue = new Deque<string>(oids);
            HashSet<string> visited = new HashSet<string>();
            while (oidsQueue.Count > 0)
            {
                string oid = oidsQueue.RemoveFromFront();
                if (string.IsNullOrWhiteSpace(oid) || visited.Contains(oid)) continue;
                visited.Add(oid);
                yield return oid;
                var commit = GetCommit(oid);
                oidsQueue.AddToFront(commit.Parent);
            }
        }
        public string GetOid(string name)
        {
            name = name == "@" ? "HEAD" : name;
            string[] refsToTry = new[]
            {
                $"{name}",
                $"refs/{name}",
                $"refs/tags/{name}",
                $"refs/heads/{name}"
            };
            foreach (var @ref in refsToTry)
            {
                if (!string.IsNullOrEmpty(data.GetRef(@ref, false).Value))
                {
                    return data.GetRef(@ref).Value;
                }
            }

            if (name.Length == 40 && name.IsOnlyHex())
            {
                return name;
            }

            Debug.Assert(false, $"Unknown name {name}");

            throw new ArgumentException($"Unknown name {name}");
        }

        private bool IsIgnore(string path)
        {
            return path.Split(fileSystem.Path.DirectorySeparatorChar).Contains(Data.GitDir);
        }
    }
}