using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Tindo.UgitCore
{
    /// <summary>
    /// Http Data provider.
    /// </summary>
    public class HttpDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;
        
        private readonly ILogger<HttpDataProvider> logger;

        private readonly IFileOperator httpFileOperator;

        public HttpDataProvider(IFileOperator httpFileOperator, ILoggerFactory loggerFactory)
        {
            this.httpFileOperator = httpFileOperator;
            this.logger = loggerFactory.CreateLogger<HttpDataProvider>();
        }
        
        public bool Exist(string path, bool isFile = true)
        {
            return this.httpFileOperator.Exists(path, isFile);
        }

        public void Write(string path, byte[] bytes)
        {
            this.httpFileOperator.Write(path, bytes);
        }

        public byte[] Read(string path)
        {
            return this.httpFileOperator.Read(path);
        }

        public void Delete(string path)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new System.NotImplementedException();
        }

        public void EmptyCurrentDirectory()
        {
            throw new System.NotImplementedException();
        }

        public string GitDirFullPath => throw new NotImplementedException();

        public string GitDir => throw new NotImplementedException();

        public Tree Index
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public void Init()
        {
            throw new System.NotImplementedException();
        }

        public string HashObject(byte[] data, string type = "blob")
        {
            throw new System.NotImplementedException();
        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            string path = $"{Constants.Objects}/{oid}";

            var data = this.httpFileOperator.Read(path);
            var index = Array.IndexOf(data, this.typeSeparator);
            if (string.IsNullOrWhiteSpace(expected) || index <= 0)
            {
                return Array.Empty<byte>();
            }

            var type = data.Take(index).ToArray().Decode();
            if (!string.Equals(expected, type, StringComparison.OrdinalIgnoreCase))
            {
                throw new UgitException($"Unknown object ({oid}) type, got {type}");
            }
            return data.TakeLast(data.Length - index - 1).ToArray();

        }

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            
        }

        public RefValue GetRef(string @ref, bool deref = true)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            throw new RowNotInTableException();
        }

        public void DeleteRef(string @ref, bool deref = true)
        {
            throw new System.NotImplementedException();
        }

        public string GetOid(string name)
        {
            throw new System.NotImplementedException();
        }

        public bool IsIgnore(string path)
        {
            throw new System.NotImplementedException();
        }

        public bool ObjectExist(string oid)
        {
            throw new System.NotImplementedException();
        }

        public Config Config { get; set; }

        public string GitFilePath => throw new NotImplementedException();
    }
}