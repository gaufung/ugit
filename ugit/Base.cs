using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Abstractions;
using System.Linq;

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
                string @type = "tree";
                string oid = WriteTree(directoryPath);
                string name = fileSystem.Path.GetRelativePath(directory, directoryPath);
                entries.Add(ValueTuple.Create(name, oid, @type));
            }

            string tree = string.Join("\n", 
                entries.Select(e => $"{e.Item3} {e.Item2} {e.Item1}"));
            return data.HashObject(tree.Encode(), "tree");
        }

        private bool IsIgnore(string path)
        {
            return path.Split(fileSystem.Path.DirectorySeparatorChar).Contains(Data.GitDir);
        }
    }
}