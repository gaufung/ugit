using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore.Operations
{
    public class TreeOperation : ITreeOperation
    {
        private readonly IDataOperator _dataOperator;

        private readonly IFileOperator _fileOperator;

        private readonly ILogger<TreeOperation> _logger;

        public TreeOperation(IDataOperator dataOperator, IFileOperator fileOperator, ILoggerFactory loggerFactory)
        {
            this._dataOperator = dataOperator;
            this._fileOperator = fileOperator;
            this._logger = loggerFactory.CreateLogger<TreeOperation>();
        }

        public void CheckoutIndex(Tree index)
        {
            this._fileOperator.EmptyDirectory(Utility.IsIgnore);
            foreach (var entry in index)
            {
                string path = entry.Key;
                string oid = entry.Value;
                this._fileOperator.Write(path, this._dataOperator.GetObject(oid, Constants.Blob));
            }
        }

        public Tree Get(string treeOid, string basePath = "")
        {
            Tree result = new();
            foreach (var (type, oid, name) in this.Iterate(treeOid))
            {
                string path = Path.Join(basePath, name);
                if (type == Constants.Blob)
                {
                    result[path] = oid;
                }
                else if (type == Constants.Tree)
                {
                    result.Update(this.Get(oid, $"{path}{Path.DirectorySeparatorChar}"));
                }
                else
                {
                    throw new UgitException($"Unknow type {type} of object {oid}");
                }
            }
            return result;
        }

        public Tree GetWorkingDirectory()
        {
            Tree result = new();
            foreach (var filePath in this._fileOperator.Walk("."))
            {
                if (Utility.IsIgnore(filePath))
                {
                    continue;
                }

                string path = Path.GetRelativePath(".", filePath);
                this._fileOperator.TryRead(path, out var bytes);
                result[path] = this._dataOperator.WriteObject(bytes);
            }

            return result;
        }

        public IEnumerable<(string, string, string)> Iterate(string oid)
        {
            if (string.IsNullOrWhiteSpace(oid))
            {
                yield break;
            }

            byte[] tree = this._dataOperator.GetObject(oid, Constants.Tree);
            foreach (string entry in tree.Decode().Split("\n"))
            {
                string[] tokens = entry.Split(' ');
                if (tokens.Length >= 3)
                {
                    yield return (tokens[0], tokens[1], tokens[2]);
                }
            }
        }

        public void Read(string treeOid, bool updateWorkingDirectory = false)
        {
            var index = this._dataOperator.Index;
            index.Clear();
            index.Update(this.Get(treeOid));
            if (updateWorkingDirectory)
            {
                this.CheckoutIndex(index);
            }
            this._dataOperator.Index = index;
        }

        public string Write()
        {
            IDictionary<string, object> indexAsTree = new Dictionary<string, object>();
            Tree index = this._dataOperator.Index;
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

            this._dataOperator.Index = index;
            return this.WriteTreeRecursive(indexAsTree);
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
                else if (entry.Value is string oid)
                {
                    string type = Constants.Blob;
                    string name = entry.Key;
                    entries.Add((name, oid, type));
                }
                else
                {
                    throw new UgitException($"Unknow tree object type: {entry.Value.GetType()}");
                }
            }

            string subTree = string.Join(
                "\n",
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return this._dataOperator.WriteObject(subTree.Encode(), Constants.Tree);
        }
    }
}
