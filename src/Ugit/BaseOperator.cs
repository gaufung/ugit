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

        private bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(dataProvider.GitDir);
    }
}
