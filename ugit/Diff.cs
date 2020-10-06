using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;

namespace ugit
{
    public class Diff
    {
        private readonly IFileSystem _fileSystem;
        private readonly Data _data;

        private readonly ICommandProcess _process;
        public Diff(IFileSystem fileSystem, Data data, ICommandProcess process)
        {
            this._fileSystem = fileSystem;
            this._data = data;
            this._process = process;
        }
        
        private IDictionary<string, string[]> CompareTrees(params IDictionary<string, string>[] trees)
        {
            IDictionary<string, string[]> output = new Dictionary<string, string[]>();
            for (int i = 0; i < trees.Length; i++)
            {
                var tree = trees[i];
                foreach (var entry in tree)
                {
                    string path = entry.Key;
                    string oid = entry.Value;
                    if (!output.ContainsKey(path))
                    {
                        output[path] = new string[trees.Length];
                    }

                    output[path][i] = oid;
                }
            }

            return output;
        }

        public string DiffTree(IDictionary<string, string> @from, IDictionary<string, string> to)
        {
            string output = "";
            foreach (var entry in CompareTrees(@from, to))
            {
                string path = entry.Key;
                string fromObject = entry.Value[0];
                string toObject = entry.Value[1];
                if (fromObject != toObject)
                {
                    output += DiffBlobs(fromObject, toObject, path);
                }
            }

            return output;
        }
        
        private string DiffBlobs(string fromObjectId, string toObjectId, string path = "blob")
        {
            string fromFile = _fileSystem.Path.GetTempFileName();
            _fileSystem.File.WriteAllBytes(fromFile, _data.GetObject(fromObjectId));
            string toFile = _fileSystem.Path.GetTempFileName();
            _fileSystem.File.WriteAllBytes(toFile, _data.GetObject(toObjectId));
            var (_, output, _) = _process.Execute(
                "diff", $"--unified --show-c-function --label a/{path} {fromFile} --label b/{path} {toFile}");
            return output;
        }

        public IEnumerable<ValueTuple<string, string>> IterChangedFiles(IDictionary<string, string> @from, IDictionary<string, string> to)
        {
            foreach (var entry in CompareTrees(@from, to))
            {
                string path = entry.Key;
                string fromObject = entry.Value[0];
                string toObject = entry.Value[1];
                if (fromObject != toObject)
                {
                    string action;
                    if (string.IsNullOrWhiteSpace(fromObject))
                    {
                        action = "new file";
                    }
                    else if (string.IsNullOrWhiteSpace(toObject))
                    {
                        action = "deleted";
                    }
                    else
                    {
                        action = "modified";
                    }

                    yield return ValueTuple.Create(path, action);
                }
            }
        }

        public IDictionary<string, string> MergeTree(IDictionary<string, string> headTree, IDictionary<string, string> otherTree)
        {
            IDictionary<string, string> tree = new Dictionary<string, string>();
            foreach (var entry in CompareTrees(headTree, otherTree))
            {
                string path = entry.Key;
                string headObject = entry.Value[0];
                string otherObject = entry.Value[1];
                Console.WriteLine($"path: {path}");
                tree[path] = _data.HashObject(MergeBlob(headObject, otherObject).Encode());
            }

            return tree;
        }
        
        private string MergeBlob(string headObjectId, string otherObjectId)
        {
            string headFile = _fileSystem.Path.GetTempFileName();
            _fileSystem.File.WriteAllBytes(headFile, _data.GetObject(headObjectId));
            string otherFile = _fileSystem.Path.GetTempFileName();
            _fileSystem.File.WriteAllBytes(otherFile, _data.GetObject(otherObjectId));
            var (_, output, _) = _process.Execute(
                "diff", $"-DHEAD {headFile} {otherFile}");
            return output;
        }
        
    }
}