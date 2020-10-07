﻿using System;
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
            throw new NotImplementedException();
        }

        public string HashObject(byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            fileSystem.Directory.CreateDirectory(GitDir);
            fileSystem.Directory.CreateDirectory(fileSystem.Path.Join(GitDir, "objects"));
        }
    }
}
