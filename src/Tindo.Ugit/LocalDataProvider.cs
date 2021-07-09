namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Default implementation of <see cref="IDataProvider"/>.
    /// </summary>
    internal class LocalDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;

        private readonly string repoPath;

        private readonly IFileOperator fileOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDataProvider"/> class.
        /// </summary>
        /// <param name="fileOperator">The file operator.</param>
        /// <param name="repoPath">repo path.</param>
        public LocalDataProvider(IFileOperator fileOperator, string repoPath)
        {
            this.fileOperator = fileOperator;
            if (string.IsNullOrWhiteSpace(repoPath))
            {
                this.repoPath = fileOperator.CurrentDirectory;
            }
            else
            {
                this.repoPath = repoPath;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDataProvider"/> class.
        /// <param name="fileOperator">The file operator.</param>
        /// </summary>
        internal LocalDataProvider(IFileOperator fileOperator)
            : this(fileOperator, string.Empty)
        {
            this.fileOperator = fileOperator;
        }

        /// <inheritdoc/>
        public string GitDir => ".ugit";

        /// <inheritdoc/>
        public string GitDirFullPath =>
            Path.Join(this.repoPath, this.GitDir);

        /// <inheritdoc/>
        public IFileOperator FileOperator => this.fileOperator;

        /// <inheritdoc/>
        public Tree Index
        {
            get
            {
                string path = Path.Join(this.GitDirFullPath, Constants.Index);
                if (this.fileOperator.TryRead(path, out var data))
                {
                    return JsonSerializer.Deserialize<Tree>(data);
                }

                return new Tree();
            }

            set
            {
                string path = Path.Join(this.GitDirFullPath, Constants.Index);
                var data = JsonSerializer.SerializeToUtf8Bytes(value);
                if (this.fileOperator.Exists(path))
                {
                    this.fileOperator.Delete(path);
                }

                this.fileOperator.Write(path, data);
            }
        }

        /// <inheritdoc/>
        public byte[] GetObject(string oid, string expected = "blob")
        {
            string filePath = Path.Join(this.GitDirFullPath, Constants.Objects, oid);
            if (this.fileOperator.TryRead(filePath, out var data))
            {
                var index = Array.IndexOf(data, this.typeSeparator);
                if (!string.IsNullOrWhiteSpace(expected) && index > 0)
                {
                    var type = data.Take(index).ToArray().Decode();
                    if (!string.Equals(expected, type, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UgitException($"Unknow object type, got {type}");
                    }

                    return data.TakeLast(data.Length - index - 1).ToArray();
                }
            }

            return Array.Empty<byte>();
        }

        /// <inheritdoc/>
        public string WriteObject(byte[] data, string type = "blob")
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                data = type.Encode().Concat(new byte[] { this.typeSeparator }).Concat(data).ToArray();
            }

            string oid = data.Sha1HexDigest();
            string filePath = Path.Join(this.GitDirFullPath, Constants.Objects, oid);
            this.fileOperator.Write(filePath, data);
            return oid;
        }

        /// <inheritdoc/>
        public void Init()
        {
            if (this.fileOperator.Exists(this.GitDirFullPath, false))
            {
                this.fileOperator.Delete(this.GitDirFullPath, false);
            }

            this.fileOperator.CreateDirectory(this.GitDirFullPath);
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

            string refDirectory = Path.Join(this.GitDirFullPath, "refs");
            foreach (var filePath in this.fileOperator.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(this.GitDirFullPath, filePath);
                if (refName.StartsWith(prefix))
                {
                    var @ref = this.GetRef(refName, deref);
                    if (!string.IsNullOrEmpty(@ref.Value))
                    {
                        yield return (refName, @ref);
                    }
                }
            }
        }

        /// <inheritdoc/>
        public void DeleteRef(string @ref, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            string filePath = Path.Join(this.GitDirFullPath, @ref);
            this.fileOperator.Delete(filePath);
        }

        /// <inheritdoc/>
        public string GetOid(string name)
        {
            name = name == "@" ? Constants.HEAD : name;
            string[] refsToTry = {
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
        public bool ObjectExist(string oid)
        {
            return this.fileOperator.Exists(Path.Join(this.GitDirFullPath, "objects", oid));
        }

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.GitDirFullPath, @ref);
            string value = null;
            if (this.fileOperator.TryRead(refPath, out var data))
            {
                value = data.Decode();
            }

            bool symbolic = !string.IsNullOrWhiteSpace(value) && value.StartsWith("ref:");
            if (symbolic)
            {
                value = value.Split(":")[1].Trim();
                if (deref)
                {
                    return this.GetRefInternal(value, true);
                }
            }

            return ValueTuple.Create(@ref, RefValue.Create(symbolic, value));
        }
    }
}
