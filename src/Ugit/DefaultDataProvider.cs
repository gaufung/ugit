namespace Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Text.Json;

    /// <summary>
    /// Default implementation of <see cref="IDataProvider"/>.
    /// </summary>
    internal class DefaultDataProvider : IDataProvider
    {
        private readonly byte typeSeparator = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public DefaultDataProvider(IFileSystem fileSystem)
        {
            this.FileSystem = fileSystem;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataProvider"/> class.
        /// </summary>
        internal DefaultDataProvider()
            : this(new FileSystem())
        {
        }

        /// <inheritdoc/>
        public string GitDir { get; } = ".ugit";

        /// <inheritdoc/>
        public string GitDirFullPath =>
            Path.Join(this.FileSystem.Directory.GetCurrentDirectory(), this.GitDir);

        /// <inheritdoc/>
        public byte[] GetObject(string oid, string expected = "blob")
        {
            string filePath = Path.Join(this.GitDir, "objects", oid);
            if (this.FileSystem.File.Exists(filePath))
            {
                var data = this.FileSystem.File.ReadAllBytes(filePath);
                var index = Array.IndexOf(data, this.typeSeparator);
                if (!string.IsNullOrWhiteSpace(expected) && index > 0)
                {
                    var type = data.Take(index).ToArray().Decode();
                    Debug.Assert(expected == type, $"expected {expected}, got {type}");
                    return data.TakeLast(data.Length - index - 1).ToArray();
                }
            }

            return Array.Empty<byte>();
        }

        /// <inheritdoc/>
        public string HashObject(byte[] data, string type = "blob")
        {
            if (!string.IsNullOrWhiteSpace(type))
            {
                data = type.Encode().Concat(new byte[] { this.typeSeparator }).Concat(data).ToArray();
            }

            string oid = data.Sha1HexDigest();
            string filePath = Path.Join(this.GitDir, "objects", oid);
            this.FileSystem.File.WriteAllBytes(filePath, data);
            return oid;
        }

        /// <inheritdoc/>
        public void Init()
        {
            if (this.FileSystem.Directory.Exists(this.GitDir))
            {
                this.FileSystem.Directory.Delete(this.GitDir, true);
            }

            this.FileSystem.Directory.CreateDirectory(this.GitDir);
            this.FileSystem.Directory.CreateDirectory(Path.Join(this.GitDir, "objects"));
        }

        /// <inheritdoc/>
        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            Debug.Assert(!string.IsNullOrEmpty(value.Value), "ref value should be null or empty");
            string val;
            if (value.Symbolic)
            {
                val = $"ref: {value.Value}";
            }
            else
            {
                val = value.Value;
            }

            string filePath = Path.Join(this.GitDir, @ref);
            this.FileSystem.CreateParentDirectory(filePath);
            this.FileSystem.File.WriteAllBytes(filePath, val.Encode());
        }

        /// <inheritdoc/>
        public RefValue GetRef(string @ref, bool deref = true)
        {
            return this.GetRefInternal(@ref, deref).Item2;
        }

        /// <inheritdoc/>
        public IEnumerable<(string, RefValue)> IterRefs(string prefix = "", bool deref = true)
        {
            if ("HEAD".StartsWith(prefix))
            {
                if (!string.IsNullOrEmpty(this.GetRef("HEAD", deref).Value))
                {
                    yield return ("HEAD", this.GetRef("HEAD", deref));
                }
            }

            if ("MERGE_HEAD".StartsWith(prefix))
            {
                if (!string.IsNullOrEmpty(this.GetRef("MERGE_HEAD", deref).Value))
                {
                    yield return ("MERGE_HEAD", this.GetRef("MERGE_HEAD", deref));
                }
            }

            string refDirectory = Path.Join(this.GitDir, "refs");
            foreach (var filePath in this.FileSystem.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(this.GitDir, filePath);
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
            string filePath = Path.Join(this.GitDir, @ref);
            if (this.FileSystem.File.Exists(filePath))
            {
                this.FileSystem.File.Delete(filePath);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetIndex()
        {
            string path = Path.Join(this.GitDir, "index");
            if (this.FileSystem.File.Exists(path))
            {
                var data = this.FileSystem.File.ReadAllBytes(path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(data);
            }

            return new Dictionary<string, string>();
        }

        /// <inheritdoc/>
        public void SetIndex(Dictionary<string, string> index)
        {
            string path = Path.Join(this.GitDir, "index");
            string data = JsonSerializer.Serialize(index);
            if (this.FileSystem.File.Exists(path))
            {
                this.FileSystem.File.Delete(path);
            }

            this.FileSystem.File.WriteAllText(path, data);
        }

        /// <inheritdoc/>
        public string GetOid(string name)
        {
            name = name == "@" ? "HEAD" : name;
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
        public IFileSystem FileSystem { get; private set; }

        /// <inheritdoc/>
        public bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(this.GitDir);

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.GitDir, @ref);
            string value = null;
            if (this.FileSystem.File.Exists(refPath))
            {
                value = this.FileSystem.File.ReadAllBytes(refPath).Decode();
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
