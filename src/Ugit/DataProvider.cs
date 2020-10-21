using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Ugit
{
    internal class DataProvider : IDataProvider
    {
        private readonly string _gitDir = ".ugit";

        private readonly byte _typeSeparator = 0;

        private readonly IFileSystem fileSystem;

        internal DataProvider() : this(new FileSystem())
        {

        }

        public DataProvider(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public string GitDir => _gitDir;

        public string GitDirFullPath => 
            Path.Join(fileSystem.Directory.GetCurrentDirectory(), GitDir);

        public byte[] GetObject(string oid, string expected="blob")
        {
            string filePath = Path.Join(GitDir, "objects", oid);
            if(fileSystem.File.Exists(filePath))
            {
                var data = fileSystem.File.ReadAllBytes(filePath);
                var index = Array.IndexOf(data, _typeSeparator);
                if(!string.IsNullOrWhiteSpace(expected) && index > 0)
                {
                    var type = data.Take(index).ToArray().Decode();
                    Debug.Assert(expected == type, $"expected {expected}, got {type}");
                    return data.TakeLast(data.Length - index - 1).ToArray();
                }
            }

            return Array.Empty<byte>();
        }

        public string HashObject(byte[] data, string type="blob")
        {
            if(!string.IsNullOrWhiteSpace(type))
            {
                data = type.Encode().Concat(new byte[] { _typeSeparator }).Concat(data).ToArray();
            }
            string oid = data.Sha1HexDigest();
            string filePath = Path.Join(GitDir, "objects", oid);
            fileSystem.File.WriteAllBytes(filePath, data);
            return oid;
        }

        public void Init()
        {
            fileSystem.Directory.CreateDirectory(GitDir);
            fileSystem.Directory.CreateDirectory(Path.Join(GitDir, "objects"));
        }

        public void UpdateRef(string @ref, RefValue value)
        {
            Debug.Assert(!value.Symbolic, "symbolic should not be true");
            string filePath = Path.Join(GitDir, @ref);
            fileSystem.CreateParentDirectory(filePath);
            fileSystem.File.WriteAllBytes(filePath, value.Value.Encode());
        }

        public RefValue GetRef(string @ref)
        {
            string filePath = Path.Join(GitDir, @ref);
            string value = null;
            if(fileSystem.File.Exists(filePath))
            {
                value = fileSystem.File.ReadAllBytes(filePath).Decode();
            }

            if(!(string.IsNullOrEmpty(value)) && value.StartsWith("ref:"))
            {
                string[] tokens = value.Split(":");
                if(tokens.Length == 2)
                {
                    return GetRef(tokens[1]);
                }
            }

            return RefValue.Create(false, value);
        }

        public IEnumerable<(string, RefValue)> IterRefs()
        {
            yield return ("HEAD", GetRef("HEAD"));
            string refDirectory = Path.Join(GitDir, "refs");
            foreach (var filePath in fileSystem.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(GitDir, filePath);
                yield return (refName, GetRef(refName));
            }
        }
    }
}
