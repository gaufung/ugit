namespace Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using Nito.Collections;

    /// <summary>
    /// Default implementation of IBaseOperator.
    /// </summary>
    internal class DefaultBaseOperator : IBaseOperator
    {
        private readonly IFileSystem fileSystem;

        private readonly IDataProvider dataProvider;

        private readonly IDiff diff;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBaseOperator"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="dataprovider">the data provider.</param>
        /// <param name="diff">The diff.</param>
        public DefaultBaseOperator(IFileSystem fileSystem, IDataProvider dataprovider, IDiff diff)
        {
            this.fileSystem = fileSystem;
            this.dataProvider = dataprovider;
            this.diff = diff;
        }

        /// <inheritdoc/>
        public string WriteTree()
        {
            IDictionary<string, object> indexAsTree = new Dictionary<string, object>();
            Dictionary<string, string> index = this.dataProvider.GetIndex();
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

            this.dataProvider.SetIndex(index);
            return this.WriteTreeRecursive(indexAsTree);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetTree(string treeOid, string basePath = "")
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var (type, oid, name) in this.IterTreeEntry(treeOid))
            {
                string path = Path.Join(basePath, name);
                if (type == "blob")
                {
                    result[path] = oid;
                }
                else if (type == "tree")
                {
                    result.Update(this.GetTree(oid, $"{path}{Path.DirectorySeparatorChar}"));
                }
                else
                {
                    throw new ArgumentException($"Unknown type {type} of object {oid}");
                }
            }

            return result;
        }

        /// <inheritdoc/>
        public void ReadTree(string treeOid, bool updateWorking = false)
        {
            var index = this.dataProvider.GetIndex();
            index.Clear();
            index.Update(this.GetTree(treeOid));
            if (updateWorking)
            {
                this.CheckoutIndex(index);
            }

            this.dataProvider.SetIndex(index);
        }

        /// <inheritdoc/>
        public string GetMergeBase(string oid1, string oid2)
        {
            IEnumerable<string> parents = this.IterCommitsAndParents(new[] { oid1 });
            foreach (var oid in this.IterCommitsAndParents(new[] { oid2 }))
            {
                if (parents.Contains(oid))
                {
                    return oid;
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public void Add(IEnumerable<string> fileNames)
        {
            var index = this.dataProvider.GetIndex();

            foreach (var name in fileNames)
            {
                if (this.fileSystem.File.Exists(name))
                {
                    this.AddFile(index, name);
                }
                else if (this.fileSystem.Directory.Exists(name))
                {
                    this.AddDirecotry(index, name);
                }
            }

            this.dataProvider.SetIndex(index);
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetIndexTree()
        {
            return this.dataProvider.GetIndex();
        }

        /// <inheritdoc/>
        public string Commit(string message)
        {
            string commit = $"tree {this.WriteTree()}\n";
            string HEAD = this.dataProvider.GetRef("HEAD").Value;
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            string mergeHead = this.dataProvider.GetRef("MERGE_HEAD").Value;
            if (!string.IsNullOrEmpty(mergeHead))
            {
                commit += $"parent {mergeHead}\n";
                this.dataProvider.DeleteRef("MERGE_HEAD", false);
            }

            commit += "\n";
            commit += $"{message}\n";

            string oid = this.dataProvider.HashObject(commit.Encode(), "commit");
            this.dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
            return oid;
        }

        /// <inheritdoc/>
        public Commit GetCommit(string oid)
        {
            var parents = new List<string>();
            var commit = this.dataProvider.GetObject(oid, "commit").Decode();
            string[] lines = commit.Split("\n");
            string tree = null;
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
                Message = message,
            };
        }

        /// <inheritdoc/>
        public void Checkout(string name)
        {
            string oid = this.GetOid(name);
            var commit = this.GetCommit(oid);
            this.ReadTree(commit.Tree, true);

            RefValue HEAD;
            if (this.IsBranch(name))
            {
                HEAD = RefValue.Create(true, Path.Join("refs", "heads", name));
            }
            else
            {
                HEAD = RefValue.Create(false, oid);
            }

            this.dataProvider.UpdateRef("HEAD", HEAD, false);
        }

        /// <inheritdoc/>
        public void CreateTag(string name, string oid)
        {
            string @ref = Path.Join("refs", "tags", name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        /// <inheritdoc/>
        public string GetOid(string name)
        {
            name = name == "@" ? "HEAD" : name;
            string[] refsToTry = new string[]
            {
                Path.Join(name),
                Path.Join("refs", name),
                Path.Join("refs", "tags", name),
                Path.Join("refs", "heads", name),
            };
            foreach (var @ref in refsToTry)
            {
                if (!string.IsNullOrEmpty(this.dataProvider.GetRef(@ref, false).Value))
                {
                    return this.dataProvider.GetRef(@ref).Value;
                }
            }

            if (name.IsOnlyHex() && name.Length == 40)
            {
                return name;
            }

            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<string> IterCommitsAndParents(IEnumerable<string> oids)
        {
            Deque<string> oidQueue = new Deque<string>(oids);
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (oidQueue.Count > 0)
            {
                string oid = oidQueue.RemoveFromFront();
                if (string.IsNullOrWhiteSpace(oid) || visited.Contains(oid))
                {
                    continue;
                }

                visited.Add(oid);
                yield return oid;

                var commit = this.GetCommit(oid);
                oidQueue.AddToFront(commit.Parents.FirstOrDefault());
                if (commit.Parents.Count > 1)
                {
                    commit.Parents.TakeLast(commit.Parents.Count - 1)
                        .ToList()
                        .ForEach(id => oidQueue.AddToBack(id));
                }
            }
        }

        /// <inheritdoc/>
        public void CreateBranch(string name, string oid)
        {
            string @ref = Path.Join("refs", "heads", name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        /// <inheritdoc/>
        public void Init()
        {
            this.dataProvider.Init();
            this.dataProvider.UpdateRef("HEAD", RefValue.Create(true, Path.Join("refs", "heads", "master")));
        }

        /// <inheritdoc/>
        public string GetBranchName()
        {
            var HEAD = this.dataProvider.GetRef("HEAD", false);
            if (!HEAD.Symbolic)
            {
                return null;
            }

            var head = HEAD.Value;
            Debug.Assert(head.StartsWith(Path.Join("refs", "heads")), "Branch ref should start with refs/heads");
            return Path.GetRelativePath(Path.Join("refs", "heads"), head);
        }

        /// <inheritdoc/>
        public IEnumerable<string> IterBranchNames()
        {
            foreach (var (refName, _) in this.dataProvider.IterRefs(Path.Join("refs", "heads")))
            {
                yield return Path.GetRelativePath(Path.Join("refs", "heads"), refName);
            }
        }

        /// <inheritdoc/>
        public void Reset(string oid)
        {
            this.dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetWorkingTree()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var filePath in this.fileSystem.Walk("."))
            {
                string path = Path.GetRelativePath(".", filePath);
                if (this.IsIgnore(path))
                {
                    continue;
                }

                result[path] = this.dataProvider.HashObject(this.fileSystem.File.ReadAllBytes(path));
            }

            return result;
        }

        /// <inheritdoc/>
        public void Merge(string other)
        {
            string head = this.dataProvider.GetRef("HEAD").Value;
            var headCommit = this.GetCommit(head);
            string mergeBase = this.GetMergeBase(other, head);
            var otherCommit = this.GetCommit(other);

            if (mergeBase == head)
            {
                this.ReadTree(otherCommit.Tree, true);
                this.dataProvider.UpdateRef("HEAD", RefValue.Create(false, other));
                Console.WriteLine("Fast-forwad, no need to commit");
                return;
            }

            this.dataProvider.UpdateRef("MERGE_HEAD", RefValue.Create(false, other));
            this.ReadTreeMerged(headCommit.Tree, otherCommit.Tree, true);
            Console.WriteLine("Merged in working tree\nPlease commit");
        }

        private string WriteTreeRecursive(IDictionary<string, object> treeDict)
        {
            List<(string, string, string)> entries = new List<(string, string, string)>();
            foreach (var entry in treeDict)
            {
                if (entry.Value is IDictionary<string, object> val)
                {
                    string type = "tree";
                    string oid = this.WriteTreeRecursive(val);
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

            string tree = string.Join(
                "\n",
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return this.dataProvider.HashObject(tree.Encode(), "tree");
        }

        private IEnumerable<(string, string, string)> IterTreeEntry(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
            {
                yield break;
            }

            byte[] tree = this.dataProvider.GetObject(oid, "tree");
            foreach (string entry in tree.Decode().Split("\n"))
            {
                string[] tokens = entry.Split(' ');
                if (tokens.Length >= 3)
                {
                    yield return (tokens[0], tokens[1], tokens[2]);
                }
            }
        }

        private void EmptyCurrentDirectory()
        {
            foreach (var filePath in this.fileSystem.Directory.EnumerateFiles("."))
            {
                if (this.IsIgnore(filePath))
                {
                    continue;
                }

                this.fileSystem.File.Delete(filePath);
            }

            foreach (var directoryPath in this.fileSystem.Directory.EnumerateDirectories("."))
            {
                if (this.IsIgnore(directoryPath))
                {
                    continue;
                }

                this.fileSystem.Directory.Delete(directoryPath, true);
            }
        }

        private void CheckoutIndex(Dictionary<string, string> index)
        {
            this.EmptyCurrentDirectory();
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                this.fileSystem.CreateParentDirectory(path);
                this.fileSystem.File.WriteAllBytes(path, this.dataProvider.GetObject(oid, "blob"));
            }
        }

        private bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(this.dataProvider.GitDir);

        private bool IsBranch(string branch)
        {
            string path = Path.Join("refs", "heads", branch);
            return !string.IsNullOrWhiteSpace(this.dataProvider.GetRef(path).Value);
        }

        private void ReadTreeMerged(string headTree, string otherTree, bool updateWorking = false)
        {
            var index = this.dataProvider.GetIndex();
            index.Clear();
            index.Update(this.diff.MergeTree(this.GetTree(headTree), this.GetTree(otherTree)));
            if (updateWorking)
            {
                this.CheckoutIndex(index);
            }

            this.dataProvider.SetIndex(index);
        }

        private void AddFile(IDictionary<string, string> index, string fileName)
        {
            var normalFileName = Path.GetRelativePath(".", fileName);
            byte[] data = this.fileSystem.File.ReadAllBytes(normalFileName);
            string oid = this.dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }

        private void AddDirecotry(IDictionary<string, string> index, string dirName)
        {
            foreach (var fileName in this.fileSystem.Walk(dirName))
            {
                if (!this.IsIgnore(fileName))
                {
                    this.AddFile(index, fileName);
                }
            }
        }
    }
}
