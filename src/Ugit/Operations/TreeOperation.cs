namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The default implementation of ITreeOperation.
    /// </summary>
    internal class TreeOperation : ITreeOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly IFileOperator fileOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="TreeOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public TreeOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            this.fileOperator = this.dataProvider.FileOperator;
        }

        /// <inheritdoc/>
        public void CheckoutIndex(Tree index)
        {
            this.fileOperator.EmptyCurrentDirectory(this.dataProvider.IsIgnore);
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                this.fileOperator.Write(path, this.dataProvider.GetObject(oid, Constants.Blob));
            }
        }

        /// <inheritdoc/>
        public Tree GetTree(string treeOid, string basePath = "")
        {
            var result = new Tree();
            foreach (var (type, oid, name) in this.IterTreeEntry(treeOid))
            {
                string path = Path.Join(basePath, name);
                if (type == Constants.Blob)
                {
                    result[path] = oid;
                }
                else if (type == Constants.Tree)
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
            var index = this.dataProvider.Index;
            index.Clear();
            index.Update(this.GetTree(treeOid));
            if (updateWorking)
            {
                this.CheckoutIndex(index);
            }

            this.dataProvider.Index = index;
        }

        /// <inheritdoc/>
        public string WriteTree()
        {
            IDictionary<string, object> indexAsTree = new Dictionary<string, object>();
            Tree index = this.dataProvider.Index;
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

            this.dataProvider.Index = index;
            return this.WriteTreeRecursive(indexAsTree);
        }

        /// <inheritdoc/>
        public Tree GetWorkingTree()
        {
            Tree result = new Tree();
            foreach (var filePath in this.fileOperator.Walk("."))
            {
                if (this.dataProvider.IsIgnore(filePath))
                {
                    continue;
                }

                string path = Path.GetRelativePath(".", filePath);
                result[path] = this.dataProvider.WriteObject(this.fileOperator.Read(path));
            }

            return result;
        }

        /// <inheritdoc/>
        public Tree GetIndexTree()
        {
            return this.dataProvider.Index;
        }

        public IEnumerable<(string, string, string)> IterTreeEntry(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
            {
                yield break;
            }

            byte[] tree = this.dataProvider.GetObject(oid, Constants.Tree);
            foreach (string entry in tree.Decode().Split("\n"))
            {
                string[] tokens = entry.Split(' ');
                if (tokens.Length >= 3)
                {
                    yield return (tokens[0], tokens[1], tokens[2]);
                }
            }
        }

        private string WriteTreeRecursive(IDictionary<string, object> tree)
        {
            List<(string, string, string)> entries = new List<(string, string, string)>();
            foreach (var entry in tree)
            {
                if (entry.Value is IDictionary<string, object> val)
                {
                    string type = Constants.Tree;
                    string oid = this.WriteTreeRecursive(val);
                    string name = entry.Key;
                    entries.Add((name, oid, type));
                }
                else
                {
                    string type = Constants.Blob;
                    string oid = entry.Value as string;
                    string name = entry.Key;
                    entries.Add((name, oid, type));
                }
            }

            string subTree = string.Join(
                "\n",
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return this.dataProvider.WriteObject(subTree.Encode(), Constants.Tree);
        }
    }
}
