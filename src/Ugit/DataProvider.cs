using System;
using System.IO;
using System.IO.Abstractions;

namespace Ugit
{
    internal class DataProvider : IDataProvider
    {
        private static readonly string _gitDir = ".ugit";

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

        public byte[] GetObject(string oid)
        {
            string filePath = Path.Join(GitDir, "objects", oid);
            if(fileSystem.File.Exists(filePath))
            {
                return fileSystem.File.ReadAllBytes(filePath);
            }

            return Array.Empty<byte>();
        }

        public string HashObject(byte[] data)
        {
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
    }
}
