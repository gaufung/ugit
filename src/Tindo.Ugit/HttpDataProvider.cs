using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tindo.Ugit
{
    internal class HttpDataProvider : IDataProvider
    {
        public string GitDirFullPath => throw new NotImplementedException();

        public string GitDir => throw new NotImplementedException();

        public IFileOperator FileOperator => throw new NotImplementedException();

        public Tree Index { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Config Config { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DeleteRef(string @ref, bool deref = true)
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

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            throw new NotImplementedException();
        }

        public string WriteObject(byte[] data, string type = "blob")
        {
            throw new NotImplementedException();
        }
    }
}
