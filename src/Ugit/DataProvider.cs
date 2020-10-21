using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace Ugit
{
    internal class DataProvider : IDataProvider
    {
        private readonly byte _typeSeparator = 0;

        private readonly IFileSystem fileSystem;

        internal DataProvider() : this(new FileSystem())
        {

        }

        public DataProvider(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public string GitDir { get; } = ".ugit";

        public string GitDirFullPath => 
            Path.Join(fileSystem.Directory.GetCurrentDirectory(), GitDir);

        public byte[] GetObject(string oid, string expected="blob")
        {
            string filePath = Path.Join(GitDir, "objects", oid);
            if(fileSystem.File.Exists(filePath))
            {
                var data = fileSystem.File.ReadAllBytes(filePath);
                var index = Array.IndexOf(data, _typeSeparator);
                if(!string.IsNullOrWhiteSpace(expected) && index > 0)
                {
                    var type = data.Take(index).ToArray().Decode();
                    Debug.Assert(expected == type, $"expected {expected}, got {type}");
                    return data.TakeLast(data.Length - index - 1).ToArray();
                }
            }

            return Array.Empty<byte>();
        }

        public string HashObject(byte[] data, string type="blob")
        {
            if(!string.IsNullOrWhiteSpace(type))
            {
                data = type.Encode().Concat(new byte[] { _typeSeparator }).Concat(data).ToArray();
            }
            string oid = data.Sha1HexDigest();
            string filePath = Path.Join(GitDir, "objects", oid);
            fileSystem.File.WriteAllBytes(filePath, data);
            return oid;
        }

        public void Init()
        {
            fileSystem.Directory.CreateDirectory(GitDir);
            fileSystem.Directory.CreateDirectory(Path.Join(GitDir, "objects"));
        }

        public void UpdateRef(string @ref, RefValue value, bool deref=true)
        {
            @ref = GetRefInternal(@ref, deref).Item1;
            Debug.Assert(!string.IsNullOrEmpty(value.Value));
            string val;
            if (value.Symbolic)
            {
                val = $"ref: {value.Value}";
            }
            else
            {
                val = value.Value;
            }

            string filePath = Path.Join(GitDir, @ref);
            fileSystem.CreateParentDirectory(filePath);
            fileSystem.File.WriteAllBytes(filePath, val.Encode());
        }

        public RefValue GetRef(string @ref, bool deref=true)
        {
            return GetRefInternal(@ref, deref).Item2;
        }

        public IEnumerable<(string, RefValue)> IterRefs(bool deref=true)
        {
            yield return ("HEAD", GetRef("HEAD"));
            string refDirectory = Path.Join(GitDir, "refs");
            foreach (var filePath in fileSystem.Walk(refDirectory))
            {
                string refName = Path.GetRelativePath(GitDir, filePath);
                yield return (refName, GetRef(refName, deref));
            }
        }

        private (string, RefValue) GetRefInternal(string @ref, bool deref)
        {
            var refPath = Path.Join(GitDir, @ref);
            string value = null;
            if (fileSystem.File.Exists(refPath))
            {
                value = fileSystem.File.ReadAllBytes(refPath).Decode();
            }
            bool symbolic = !string.IsNullOrWhiteSpace(value) && value.StartsWith("ref:");
            if(symbolic)
            {
                value = value.Split(":")[1].Trim();
                if(deref)
                {
                    return GetRefInternal(value, true);
                }
            }

            return ValueTuple.Create(@ref, RefValue.Create(symbolic, value));
        }
    }
}
