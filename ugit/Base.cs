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

        public Base(Data data, IFileSystem fileSystem)
        {
            this.data = data;
            this.fileSystem = fileSystem;
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

        private IDictionary<string, string> GetTree(string oid, string basePath="")
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

        public string Commit(string message)
        {
            string commit = $"tree {WriteTree()}\n";
            string HEAD = data.GetRef("HEAD");
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = data.HashObject(commit.Encode(), "commit");
            data.UpdateRef("HEAD",oid);
            return oid;
        }

        public void CheckOut(string oid)
        {
            var commit = GetCommit(oid);
            ReadTree(commit.Tree);
            data.UpdateRef("HEAD", oid);
        }

        public void CreateTag(string name, string oid)
        {
            string @ref = fileSystem.Path.Join("refs", "tags", name);
            data.UpdateRef(@ref, oid);
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
                if (!string.IsNullOrEmpty(data.GetRef(@ref)))
                {
                    return data.GetRef(@ref);
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