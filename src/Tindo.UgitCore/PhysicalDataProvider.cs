using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.FileProviders;

namespace Tindo.UgitCore
{
    public class PhysicalDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;

        private readonly IFileProvider fileProvider;

        private readonly string repoPath;

        public PhysicalDataProvider(IFileProvider fileProvider, string repoPath)
        {
            this.fileProvider = fileProvider;
            this.repoPath = repoPath;
        }

        public string GitDirFullPath => this.fileProvider.GetFileInfo(Constants.GitDir).PhysicalPath;

        public string GitDir => Constants.GitDir;

        public Tree Index { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Delete(string path)
        {
            throw new NotImplementedException();
        }

        public void DeleteRef(string @ref, bool deref = true)
        {
            throw new NotImplementedException();
        }

        public void EmptyCurrentDirectory()
        {
            throw new NotImplementedException();
        }

        public bool Exist(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            throw new NotImplementedException();
        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            throw new NotImplementedException();
        }

        public string GetOid(string name)
        {
            throw new NotImplementedException();
        }

        public RefValue GetRef(string @ref, bool deref = true)
        {
            throw new NotImplementedException();
        }

        public string HashObject(byte[] data, string type = "blob")
        {
            throw new NotImplementedException();
        }

        public void Init()
        {
            throw new NotImplementedException();
        }

        public bool IsIgnore(string path)
        {
            throw new NotImplementedException();
        }

        public bool ObjectExist(string oid)
        {
            throw new NotImplementedException();
        }

        public byte[] Read(string path)
        {
            throw new NotImplementedException();
        }

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new NotImplementedException();
        }

        public void Write(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
