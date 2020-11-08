﻿namespace Ugit
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

        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDataProvider"/> class.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        public DefaultDataProvider(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
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
            Path.Join(this.fileSystem.Directory.GetCurrentDirectory(), this.GitDir);

        public Dictionary<string, string> Index 
        { 
            get 
            {
                string path = Path.Join(this.GitDir, "index");
                if (this.fileSystem.File.Exists(path))
                {
                    var data = this.fileSystem.File.ReadAllBytes(path);
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(data);
                }
                
                return new Dictionary<string, string>();
            } 
            set
            {
                string path = Path.Join(this.GitDir, "index");
                string data  = JsonSerializer.Serialize(value);
                if (this.fileSystem.File.Exists(path))
                {
                    this.fileSystem.File.Delete(path);
                }

                this.fileSystem.File.WriteAllText(path, data);
            }
        }

        /// <inheritdoc/>
        public byte[] GetObject(string oid, string expected = "blob")
        {
            string filePath = Path.Join(this.GitDir, "objects", oid);
            if (this.fileSystem.File.Exists(filePath))
            {
                var data = this.fileSystem.File.ReadAllBytes(filePath);
                var index = Array.IndexOf(data, this.typeSeparator);
                if (!string.IsNullOrWhiteSpace(expected) && index > 0)
                {
                    var type = data.Take(index).ToArray().Decode();
                    if (!string.Equals(expected, type, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new ArgumentException($"Unknow object type, got {type}");
                    }

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
            this.fileSystem.File.WriteAllBytes(filePath, data);
            return oid;
        }

        /// <inheritdoc/>
        public void Init()
        {
            if (this.fileSystem.Directory.Exists(this.GitDir))
            {
                this.fileSystem.Directory.Delete(this.GitDir, true);
            }

            this.fileSystem.Directory.CreateDirectory(this.GitDir);
            this.fileSystem.Directory.CreateDirectory(Path.Join(this.GitDir, "objects"));
        }

        /// <inheritdoc/>
        public void UpdateRef(string @ref, RefValue value, bool deref = true)
        {
            @ref = this.GetRefInternal(@ref, deref).Item1;
            if (string.IsNullOrWhiteSpace(value.Value))
            {
                throw new ArgumentException("ref value could be null or empty");
            }

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
            this.fileSystem.CreateParentDirectory(filePath);
            this.fileSystem.File.WriteAllBytes(filePath, val.Encode());
        }

        /// <inheritdoc/>
        public RefValue GetRef(string @ref, bool deref = true)
        {
            return this.GetRefInternal(@ref, deref).Item2;
        }

        /// <inheritdoc/>
        public IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true)
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
            foreach (var filePath in this.fileSystem.Walk(refDirectory))
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
            if (this.fileSystem.File.Exists(filePath))
            {
                this.fileSystem.File.Delete(filePath);
            }
        }

        /// <inheritdoc/>
        public Dictionary<string, string> GetIndex()
        {
            string path = Path.Join(this.GitDir, "index");
            if (this.fileSystem.File.Exists(path))
            {
                var data = this.fileSystem.File.ReadAllBytes(path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(data);
            }

            return new Dictionary<string, string>();
        }

        /// <inheritdoc/>
        public void SetIndex(Dictionary<string, string> index)
        {
            string path = Path.Join(this.GitDir, "index");
            string data = JsonSerializer.Serialize(index);
            if (this.fileSystem.File.Exists(path))
            {
                this.fileSystem.File.Delete(path);
            }

            this.fileSystem.File.WriteAllText(path, data);
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
        public bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(this.GitDir);

        /// <inheritdoc/>
        public bool Exist(string path, bool isFile = true)
        {
            if (isFile)
            {
                return this.fileSystem.File.Exists(path);
            }
            else
            {
                return this.fileSystem.Directory.Exists(path);
            }
        }

        /// <inheritdoc/>
        public void WriteAllBytes(string path, byte[] bytes)
        {
            this.fileSystem.CreateParentDirectory(path);
            this.fileSystem.File.WriteAllBytes(path, bytes);
        }

        /// <inheritdoc/>
        public byte[] ReadAllBytes(string path)
        {
            return this.fileSystem.File.ReadAllBytes(path);
        }

        /// <inheritdoc/>
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

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(this.GitDir, @ref);
            string value = null;
            if (this.fileSystem.File.Exists(refPath))
            {
                value = this.fileSystem.File.ReadAllBytes(refPath).Decode();
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
