using System.Collections.Generic;
using System.Text.Json;

namespace Tindo.UgitCore
{
    using System;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;

    public class LocalDataOperator : IDataOperator
    {
        private readonly string repoRootPath;

        private readonly ILogger<LocalDataOperator> logger;

        private readonly string repoUgitPath;

        private readonly IFileOperator localFileOperator;

        public LocalDataOperator(IFileOperator localFileOperator, string repoRootPath, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<LocalDataOperator>();
            this.localFileOperator = localFileOperator;
            if (string.IsNullOrEmpty(repoRootPath))
            {
                repoRootPath = this.localFileOperator.CurrentDirectory;
            }

            this.repoRootPath = repoRootPath;
            this.repoUgitPath = Path.Join(this.repoRootPath, Constants.GitDir);
        }

        public LocalDataOperator(IFileOperator localFileOpeator, ILoggerFactory loggerFactory)
            : this(localFileOpeator, "", loggerFactory)
        {

        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            string objectFilePath = Path.Join(this.repoUgitPath, Constants.Objects, oid);
            if (!this.localFileOperator.TryRead(objectFilePath, out var data))
            {
                return Array.Empty<byte>();
            }

            var index = Array.IndexOf(data, Constants.TypeSeparator);
            if (index < 0)
            {
                return Array.Empty<byte>();
            }

            var actualType = data.Take(index).ToArray().Decode();
            if (!string.Equals(expected, actualType, StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogError($"Failed to read object {oid}, expect {expected} but got {actualType}");
                throw new UgitException($"Failed to read object {oid}, expect {expected} but got {actualType}");
            }

            return data.TakeLast(data.Length - index - 1).ToArray();
        }

        public string WriteObject(byte[] data, string type = "blob")
        {
            if (!string.IsNullOrEmpty(type))
            {
                data = type.Encode().Concat(new byte[] {Constants.TypeSeparator}).Concat(data).ToArray();
            }

            string oid = data.Sha1HexDigest();
            string objectFilePath = Path.Join(this.repoUgitPath, Constants.Objects, oid);
            this.localFileOperator.Write(objectFilePath, data);
            return oid;
        }

        public void Initialize()
        {
            this.localFileOperator.CreateDirectory(this.repoUgitPath);
            string objectDirectory = Path.Join(this.repoUgitPath, Constants.Objects);
            this.localFileOperator.CreateDirectory(objectDirectory);
            string headsDirectory = Path.Join(this.repoUgitPath, Constants.Refs, Constants.Heads);
            this.localFileOperator.CreateDirectory(headsDirectory);
            string tagsDirectory = Path.Join(this.repoUgitPath, Constants.Refs, Constants.Tags);
            this.localFileOperator.CreateDirectory(tagsDirectory);

        }

        public string RepositoryPath => this.repoUgitPath;

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.repoUgitPath, @ref);

            if (!this.localFileOperator.TryRead(refPath, out var bytes))
            {
                return ValueTuple.Create(@ref, RefValue.Create(false, string.Empty));
            }

            string value = bytes.Decode();
            bool symbolic = !string.IsNullOrWhiteSpace(value) && value.StartsWith("ref:");
            if (!symbolic)
            {
                return ValueTuple.Create(@ref, RefValue.Create(false, value));
            }

            value = value.Split(":")[1].Trim();
            return deref ? 
                this.GetRefInternal(value, true) : 
                ValueTuple.Create(@ref, RefValue.Create(true, value));
        }

        public RefValue GetRef(string @ref, bool deref = true)
        {
            return this.GetRefInternal(@ref, deref).Item2;
        }

        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                this.logger.LogError("RefValue is null or empty to update");
                throw new UgitException("RefValue is null or empty");
            }

            var val = value.Symbolic ? $"ref: {value.Value}" : value.Value;
            string refPath = Path.Join(this.repoUgitPath, @ref);
            this.localFileOperator.Write(refPath, val.Encode());
        }

        public void DeleteRef(string @ref, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;

            string filePath = Path.Join(this.repoUgitPath, @ref);
            if (this.localFileOperator.Exists(filePath))
            {
                this.localFileOperator.Delete(filePath);
            }
        }

        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
        {
            if (Constants.HEAD.StartsWith(prefix))
            {
                if (!string.IsNullOrEmpty(this.GetRef(Constants.HEAD, deref).Value))
                {
                    yield return (Constants.HEAD, this.GetRef(Constants.HEAD, deref));
                }
            }

            if (Constants.MergeHEAD.StartsWith(prefix))
            {
                if (!string.IsNullOrEmpty(this.GetRef(Constants.MergeHEAD, deref).Value))
                {
                    yield return (Constants.MergeHEAD, this.GetRef(Constants.MergeHEAD, deref));
                }
            }

            string refsDirectory = Path.Join(this.repoUgitPath, Constants.Refs);
            foreach (var filePath in this.localFileOperator.Walk(refsDirectory))
            {
                this.logger.LogInformation($"getting ref file path {filePath}, prefix: {prefix}");
                string refName = Path.GetRelativePath(this.repoUgitPath, filePath);
                if (!refName.StartsWith(prefix)) continue;
                
                this.logger.LogInformation($"Getting ref name {refName}");
                var refValue = this.GetRef(refName, deref);
                if (!string.IsNullOrEmpty(refValue.Value))
                {
                    yield return (refName, refValue);
                }
            }
        }

        public string GetOid(string name)
        {
            name = name == "@" ? Constants.HEAD : name;
            string[] refsToTry = new[]
            {
                Path.Join(name),
                Path.Join(Constants.Refs, name),
                Path.Join(Constants.Refs, Constants.Tags, name),
                Path.Join(Constants.Refs, Constants.Heads, name)
            };
            foreach (var @ref in refsToTry)
            {
                if (!this.GetRef(@ref, false).Value.IsNullOrWhiteSpace())
                {
                    return this.GetRef(@ref).Value;
                }
            }

            if (name.IsOnlyHex() && name.Length == 40)
            {
                return name;
            }

            return null;
        }

        public Tree Index
        {
            get
            {
                string filePath = Path.Join(this.repoUgitPath, Constants.Index);
                if (this.localFileOperator.TryRead(filePath, out var bytes))
                {
                    return JsonSerializer.Deserialize<Tree>(bytes);
                }

                return new();
            }

            set
            {
                string filePath = Path.Join(this.repoUgitPath, Constants.Index);
                var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
                this.localFileOperator.Write(filePath, bytes);
            }
        }

        public Configuration Config 
        {
            get
            {
                string filePath = Path.Join(this.repoRootPath, Constants.Config);
                if (this.localFileOperator.TryRead(filePath, out var bytes))
                {
                    return JsonSerializer.Deserialize<Configuration>(bytes);
                }
                return new();
            }
            set
            {
                string filePath = Path.Join(this.repoRootPath, Constants.Config);
                var bytes = JsonSerializer.SerializeToUtf8Bytes(value);
                this.localFileOperator.Write(filePath, bytes);
            }
        }
    }
}
