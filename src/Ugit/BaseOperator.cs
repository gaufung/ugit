using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Ugit
{
    internal class BaseOperator : IBaseOperator
    {
        private readonly IFileSystem fileSystem;

        private readonly IDataProvider dataProvider;

        public BaseOperator(IFileSystem fileSystem, IDataProvider dataprovider)
        {
            this.fileSystem = fileSystem;
            this.dataProvider = dataprovider;
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
                yield return (tokens[0], tokens[1], tokens[2]);
            }
        }

        private IDictionary<string, string> GetTree(string treeOid, string basePath = "")
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
            string HEAD = dataProvider.GetRef("HEAD");
            if(!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            commit += "\n";
            commit += $"{message}\n";

            string oid = dataProvider.HashObject(commit.Encode(), "commit");
            dataProvider.UpdateRef("HEAD", oid);
            return oid;
        }

        public Commit GetCommit(string oid)
        {
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
                    parent = tokens[1];
                }
            }

            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parent = parent,
                Message = message
            };
        }

        public void Checkout(string oid)
        {
            var commit = GetCommit(oid);
            ReadTree(commit.Tree);
            dataProvider.UpdateRef("HEAD", oid);
        }

        public void CreateTag(string name, string oid)
        {
            string @ref = Path.Join("refs", "tags", name);
            dataProvider.UpdateRef(@ref, oid);
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
                if(!string.IsNullOrEmpty(dataProvider.GetRef(@ref)))
                {
                    return dataProvider.GetRef(@ref);
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
            HashSet<string> oidSet = new HashSet<string>(oids, StringComparer.OrdinalIgnoreCase);
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while(oidSet.Count > 0)
            {
                string oid = oidSet.Pop();
                if (string.IsNullOrWhiteSpace(oid) || visited.Contains(oid)) continue;
                visited.Add(oid);
                yield return oid;

                var commit = GetCommit(oid);
                oidSet.Add(commit.Parent);
            }
        }
    }
}
