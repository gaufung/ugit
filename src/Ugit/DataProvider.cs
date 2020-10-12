using System;
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

        public void SetHEAD(string oid)
        {
            string filePath = Path.Join(GitDir, "HEAD");
            fileSystem.File.WriteAllBytes(filePath, oid.Encode());
        }
    }
}
