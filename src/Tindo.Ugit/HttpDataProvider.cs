namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.Json;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// HttpDataProvider.
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

        /// <summary>
        /// Get all refs from the remote server.
        /// </summary>
        /// <param name="prefix">the prefix.</param>
        /// <param name="deref">dereference.</param>
        /// <returns>The values.</returns>
        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            prefix = Uri.EscapeDataString(prefix);
            string path = string.IsNullOrWhiteSpace(prefix) ?
                $"refs?deref={deref}" : $"refs?deref={deref}&prefix={prefix}";
            byte[] data = this.FileOperator.Read(path);
            return JsonSerializer.Deserialize<Dictionary<string, RefValue>>(data)
                .Select(kvp => (kvp.Key, kvp.Value));
        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            string path = string.IsNullOrEmpty(expected) ? $"objects/{oid}" : $"objects/{oid}?expected={expected}";
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

        public byte[] ReadObject(string oid)
        {
            string path = $"objects/{oid}";
            return this.FileOperator.Read(path);
        }

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            string path = $"ref/{@ref}?deref={deref}";
            byte[] data = JsonSerializer.SerializeToUtf8Bytes(value);
            this.FileOperator.Write(path, data);
        }

        public string WriteObject(byte[] data, string type = "blob")
        {
            throw new NotImplementedException();
        }

        public void WriteObject(string oid, byte[] data)
        {
            string path = $"objects/{oid}";
            this.FileOperator.Write(path, data);
        }
    }
}
