namespace Tindo.UgitCore
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Default implementation of <see cref="IDataProvider"/>.
    /// </summary>
    public class LocalDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;

        private readonly string repoPath;

        private readonly ILogger<LocalDataProvider> logger;

        private readonly string ugitDirectoryFullPath;

        private readonly IFileOperator fileOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDataProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="repoPath">repo path.</param>
        public LocalDataProvider(IFileOperator phyiscalFileOperator, string repoPath, ILoggerFactory loggerFactotry)
        {
            this.fileOperator = phyiscalFileOperator;
            this.repoPath = string.IsNullOrWhiteSpace(repoPath) ?
                this.fileOperator.CurrentDirectory :
                repoPath;
            this.ugitDirectoryFullPath = Path.Join(this.repoPath, Constants.GitDir);
            this.logger = loggerFactotry.CreateLogger<LocalDataProvider>();
            
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDataProvider"/> class.
        /// </summary>
        public LocalDataProvider(IFileOperator phyiscalFileOperator, ILoggerFactory loggerFactory)
            : this(phyiscalFileOperator, "", loggerFactory)
        {
        }

        /// <inheritdoc/>
        public Tree Index
        {
            get
            {
                string path = Path.Join(this.ugitDirectoryFullPath, Constants.Index);
                if (this.fileOperator.TryRead(path, out var bytes))
                {
                    var options = new JsonSerializerOptions();
                    options.Converters.Add(new TreeJsonConverter());
                    return JsonSerializer.Deserialize<Tree>(bytes, options);
                }
                return new();
            }

            set
            {
                string path = Path.Join(this.ugitDirectoryFullPath, Constants.Index);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new TreeJsonConverter());
                string data = JsonSerializer.Serialize(value, options);
                this.fileOperator.Write(path, data.Encode());
            }
        }

        /// <inheritdoc/>
        public byte[] GetObject(string oid, string expected = "blob")
        {
            string filePath = Path.Join(this.ugitDirectoryFullPath, Constants.Objects, oid);
            if (!this.fileOperator.Exists(filePath, true))
            {
                return Array.Empty<byte>();
            }

            var data = this.fileOperator.Read(filePath);
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

        /// <inheritdoc/>
        public string HashObject(byte[] data, string type = "blob")
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                data = type.Encode().Concat(new byte[] { this.typeSeparator }).Concat(data).ToArray();
            }

            string oid = data.Sha1HexDigest();
            string filePath = Path.Join(this.repoPath, Constants.GitDir, Constants.Objects, oid);
            this.fileOperator.Write(filePath, data);
            return oid;
        }

        /// <inheritdoc/>
        public void Init()
        {
            if (this.fileOperator.Exists(this.ugitDirectoryFullPath, false))
            {
                this.fileOperator.Delete(this.ugitDirectoryFullPath);
            }

            this.fileOperator.CreateDirectory(this.ugitDirectoryFullPath);
            this.fileOperator.CreateDirectory(Path.Join(this.ugitDirectoryFullPath, Constants.Objects));
        }

        /// <inheritdoc/>
        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                throw new ArgumentException("ref value could be null or empty");
            }

            var val = value.Symbolic ? $"ref: {value.Value}" : value.Value;
            string filePath = Path.Join(this.ugitDirectoryFullPath, @ref);
            this.fileOperator.Write(filePath, val.Encode());
        }

        /// <inheritdoc/>
        public RefValue GetRef(string @ref, bool deref = true)
        {
            return this.GetRefInternal(@ref, deref).Item2;
        }

        /// <inheritdoc/>
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

            string refDirectory = Path.Join(this.ugitDirectoryFullPath, Constants.Refs);
            foreach (var filePath in this.fileOperator.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(this.ugitDirectoryFullPath, filePath);
                if (!refName.StartsWith(prefix))
                {
                    continue;
                }

                var @ref = this.GetRef(refName, deref);
                if (!string.IsNullOrEmpty(@ref.Value))
                {
                    yield return (refName, @ref);
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteRef(string @ref, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            string filePath = Path.Join(this.ugitDirectoryFullPath, @ref);
            if (this.fileOperator.Exists(filePath))
            {
                this.fileOperator.Delete(filePath);
            }
        }

        /// <inheritdoc/>
        public string GetOid(string name)
        {
            name = name == "@" ? Constants.HEAD : name;
            string[] refsToTry = new string[]
            {
                Path.Join(name),
                Path.Join("refs", name),
                Path.Join("refs", "tags", name),
                Path.Join("refs", "heads", name),
            };
            foreach (var @ref in refsToTry)
            {
                if (!string.IsNullOrEmpty(this.GetRef(@ref, false).Value))
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

        /// <inheritdoc/>
        public bool IsIgnore(string path) => 
            path.Split(Path.DirectorySeparatorChar).Contains(this.ugitDirectoryFullPath);

        public void EmptyFolder() => this.fileOperator.EmptyCurrentDirectory(IsIgnore);

        /// <inheritdoc/>
        public bool ObjectExist(string oid)
        {
            return this.fileOperator.Exists(Path.Join(this.ugitDirectoryFullPath, Constants.Objects, oid));
        }

        public Config Config
        {
            get
            {
                string filePath = Path.Join(this.ugitDirectoryFullPath, Constants.Config);

                if (this.fileOperator.TryRead(filePath, out var bytes))
                {
                    return JsonSerializer.Deserialize<Config>(bytes);
                }

                return new();
                
            }

            set
            {
                string relativePath = Path.Join(this.ugitDirectoryFullPath, Constants.Config);
                string data = JsonSerializer.Serialize(value);
                this.fileOperator.Write(relativePath, data.Encode());
            }
        }

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.ugitDirectoryFullPath, @ref);
            string value = null;
            if (this.fileOperator.Exists(refPath))
            {
                value = this.fileOperator.Read(refPath).Decode();
            }

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

        public string GitFilePath
        {
            get
            {
                return this.ugitDirectoryFullPath;
            }
        }
    }
}