namespace Ugit.Operations
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The default implementation of ITreeOperation.
    /// </summary>
    internal class DefaultTreeOperation : ITreeOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTreeOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultTreeOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public void CheckoutIndex(Dictionary<string, string> index)
        {
            this.dataProvider.EmptyCurrentDirectory();
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                this.dataProvider.WriteAllBytes(path, this.dataProvider.GetObject(oid, "blob"));
            }
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
        public IDictionary<string, string> GetWorkingTree()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            foreach (var filePath in this.dataProvider.Walk("."))
            {
                string path = Path.GetRelativePath(".", filePath);
                if (this.dataProvider.IsIgnore(path))
                {
                    continue;
                }

                result[path] = this.dataProvider.HashObject(this.dataProvider.ReadAllBytes(path));
            }

            return result;
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetIndexTree()
        {
            return this.dataProvider.GetIndex();
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

        private string WriteTreeRecursive(IDictionary<string, object> tree)
        {
            List<(string, string, string)> entries = new List<(string, string, string)>();
            foreach (var entry in tree)
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

            string subTree = string.Join(
                "\n",
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return this.dataProvider.HashObject(subTree.Encode(), "tree");
        }
    }
}
