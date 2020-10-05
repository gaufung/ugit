using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;

namespace ugit
{
    public class Data
    {
        private readonly IFileSystem _fileSystem;

        internal static string GitDir = ".ugit";

        private static byte TypeSeparator = 0;
        
        public Data(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public Data() : this(new FileSystem())
        {
            
        }

        public string GitDirPath => _fileSystem.Path.Join(_fileSystem.Directory.GetCurrentDirectory(), GitDir);

        public void Init()
        {
            _fileSystem.Directory.CreateDirectory(GitDir);
            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Join(GitDir, "objects"));
        }

        public void UpdateRef(string @ref, RefValue value, bool deref=true)
        {
            @ref = GetRefInternal(@ref, deref).Item1;

            string val;
            if (value.Symbolic)
            {
                val = $"ref: {value.Value}";
            }
            else
            {
                val = value.Value;
            }
            string refPath = _fileSystem.Path.Join(GitDir, @ref);
            refPath.CreateParentDirectory(_fileSystem);
            _fileSystem.File.WriteAllText(refPath, val);
        }

        public RefValue GetRef(string @ref, bool deref=true)
        {
            var (_, result) = GetRefInternal(@ref, deref);
            return result;
        }

        private ValueTuple<string, RefValue> GetRefInternal(string @ref, bool deref)
        {
            string refPath = _fileSystem.Path.Join(GitDir, @ref);
            string value = null;
            if (_fileSystem.File.Exists(refPath))
            {
                value = _fileSystem.File.ReadAllBytes(refPath).Decode();
            }

            bool symbolic = !string.IsNullOrWhiteSpace(value) && value.StartsWith("ref:");
            if (symbolic)
            {
                value = value.Split(':', 2)[1].Trim();
                if (deref)
                {
                    return GetRefInternal(value, true);
                }
            }

            return ValueTuple.Create(@ref, RefValue.Create(symbolic, value));
        }

        public IEnumerable<ValueTuple<string, RefValue>> IterRefs(string prefix="", bool deref=true)
        {
            List<string> refs = new List<string>(){"HEAD"};
            string directory = _fileSystem.Path.Join(GitDir, "refs");
            foreach (var filePath in _fileSystem.Walk(directory))
            {
                refs.Add(_fileSystem.Path.GetRelativePath(GitDir, filePath));
            }

            return refs.Where(@ref => @ref.StartsWith(prefix))
                .Select(@ref => ValueTuple.Create(@ref, GetRef(@ref, deref)));
        }

        public string HashObject(byte[] data, string type="blob")
        {
            byte[] obj = type.Encode().Concat(new[] {TypeSeparator}).Concat(data).ToArray();
            string oid = obj.Sha1HexDigest();
            string filePath = _fileSystem.Path.Join(GitDir, "objects", oid);
            filePath.CreateParentDirectory(_fileSystem);
            _fileSystem.File.WriteAllBytes(filePath, obj);
            return oid;
        }

        public byte[] GetObject(string oid, string expected="blob")
        {
            string filePath = _fileSystem.Path.Join(GitDir, "objects", oid);
            var obj = _fileSystem.File.ReadAllBytes(filePath);
            var index = Array.IndexOf(obj, TypeSeparator);
            if (!string.IsNullOrWhiteSpace(expected))
            {
                var type = obj.Take(index).ToArray().Decode();
                Debug.Assert(expected == type, $"expected {expected}, got {type}");
            }
            return obj.TakeLast(obj.Length - index - 1).ToArray();
        }
    }
}