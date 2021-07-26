using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Tindo.Ugit
{
    /// <summary>
    /// HttpDataProvider 
    /// </summary>
    internal class HttpDataProvider : IDataProvider
    {
        private ILogger<HttpDataProvider> _logger;

        public HttpDataProvider(IFileOperator fileOperator, ILogger<HttpDataProvider> logger)
        {
            FileOperator = fileOperator;
            _logger = logger;
        }

        public string GitDirFullPath => string.Empty;

        public string GitDir => throw new NotImplementedException();

        public IFileOperator FileOperator { get; private set; }

        public Tree Index { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public Config Config { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void DeleteRef(string @ref, bool deref = true)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            string path = string.IsNullOrWhiteSpace(prefix) ?
                $"refs?deref={deref}" : $"refs?deref={deref}&prefix={prefix}";
            byte[] data = this.FileOperator.Read(path);
            return JsonSerializer.Deserialize<List<(string, RefValue)>>(data);
        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            string path = $"objects/{oid}?expected={expected}";
            return this.FileOperator.Read(path);
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
