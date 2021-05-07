namespace Tindo.UgitCore
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Default implementation of <see cref="IDataProvider"/>.
    /// </summary>
    public class DefaultDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;

        private readonly IFileSystem fileSystem;

        private readonly string repoPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="repoPath">repo path.</param>
        public DefaultDataProvider(IFileSystem fileSystem, string repoPath = "")
        {
            this.fileSystem = fileSystem;
            this.repoPath = string.IsNullOrWhiteSpace(repoPath) ?
                this.fileSystem.Directory.GetCurrentDirectory() :
                repoPath;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataProvider"/> class.
        /// </summary>
        public DefaultDataProvider()
            : this(new FileSystem())
        {
        }

        /// <inheritdoc/>
        public string GitDir => Constants.GitDir;

        /// <inheritdoc/>
        public string GitDirFullPath =>
            Path.Join(this.repoPath, this.GitDir);

        /// <inheritdoc/>
        public Tree Index
        {
            get
            {
                string path = Path.Join(this.GitDirFullPath, Constants.Index);
                if (!this.fileSystem.File.Exists(path))
                {
                    return new();
                }

                var data = this.Read(path);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new TreeJsonConverter());
                return JsonSerializer.Deserialize<Tree>(data, options);
            }

            set
            {
                string path = Path.Join(this.GitDirFullPath, Constants.Index);
                var options = new JsonSerializerOptions();
                options.Converters.Add(new TreeJsonConverter());
                string data = JsonSerializer.Serialize(value, options);
                if (this.fileSystem.File.Exists(path))
                {
                    this.fileSystem.File.Delete(path);
                }

                this.Write(path, data.Encode());
            }
        }

        /// <inheritdoc/>
        public byte[] GetObject(string oid, string expected = "blob")
        {
            string filePath = Path.Join(this.GitDirFullPath, Constants.Objects, oid);
            if (!this.Exist(filePath, true))
            {
                return Array.Empty<byte>();
            }

            var data = this.Read(filePath);
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
            string filePath = Path.Join(this.GitDirFullPath, Constants.Objects, oid);
            this.Write(filePath, data);
            return oid;
        }

        /// <inheritdoc/>
        public void Init()
        {
            if (this.fileSystem.Directory.Exists(this.GitDirFullPath))
            {
                this.fileSystem.Directory.Delete(this.GitDirFullPath, true);
            }

            this.fileSystem.Directory.CreateDirectory(this.GitDirFullPath);
            this.fileSystem.Directory.CreateDirectory(Path.Join(this.GitDirFullPath, Constants.Objects));
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
            string filePath = Path.Join(this.GitDirFullPath, @ref);
            this.fileSystem.CreateParentDirectory(filePath);
            this.Write(filePath, val.Encode());
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

            string refDirectory = Path.Join(this.GitDirFullPath, Constants.Refs);
            foreach (var filePath in this.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(this.GitDirFullPath, filePath);
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
            string filePath = Path.Join(this.GitDirFullPath, @ref);
            if (this.fileSystem.File.Exists(filePath))
            {
                this.fileSystem.File.Delete(filePath);
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
        public bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(this.GitDir);

        /// <inheritdoc/>
        public bool Exist(string path, bool isFile = true)
        {
            return isFile ?
                this.fileSystem.File.Exists(path) :
                this.fileSystem.Directory.Exists(path);
        }

        /// <inheritdoc/>
        public void Write(string path, byte[] bytes)
        {
            this.fileSystem.CreateParentDirectory(path);
            this.fileSystem.File.WriteAllBytes(path, bytes);
        }

        /// <inheritdoc/>
        public byte[] Read(string path)
        {
            return this.fileSystem.File.ReadAllBytes(path);
        }
        
        public IEnumerable<string> Walk(string path)
        {
            return this.fileSystem.Walk(path);
        }

        /// <inheritdoc/>
        public void EmptyCurrentDirectory()
        {
            foreach (var filePath in this.fileSystem.Directory.EnumerateFiles("."))
            {
                if (this.IsIgnore(filePath))
                {
                    continue;
                }

                this.fileSystem.File.Delete(filePath);
            }

            foreach (var directoryPath in this.fileSystem.Directory.EnumerateDirectories("."))
            {
                if (this.IsIgnore(directoryPath))
                {
                    continue;
                }

                this.fileSystem.Directory.Delete(directoryPath, true);
            }
        }

        /// <inheritdoc/>
        public void Delete(string path)
        {
            if (this.IsIgnore(path))
            {
                return;
            }

            this.fileSystem.File.Delete(path);
        }

        /// <inheritdoc/>
        public bool ObjectExist(string oid)
        {
            return this.fileSystem.File.Exists(Path.Join(this.GitDirFullPath, Constants.Objects, oid));
        }

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.GitDirFullPath, @ref);
            string value = null;
            if (this.fileSystem.File.Exists(refPath))
            {
                value = this.fileSystem.File.ReadAllBytes(refPath).Decode();
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
    }
}