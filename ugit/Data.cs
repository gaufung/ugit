using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        public void DeleteRef(string @ref, bool deref = true)
        {
            @ref = GetRefInternal(@ref, deref).Item1;
            string path = _fileSystem.Path.Join(GitDir, @ref);
            if (_fileSystem.File.Exists(path))
            {
                _fileSystem.File.Delete(path);
            }
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
            List<string> refs = new List<string>(){"HEAD", "MERGE_HEAD"};
            string directory = _fileSystem.Path.Join(GitDir, "refs");
            foreach (var filePath in _fileSystem.Walk(directory))
            {
                refs.Add(_fileSystem.Path.GetRelativePath(GitDir, filePath));
            }

            foreach (var refName in refs)
            {
                if(!refName.StartsWith(prefix))
                    continue;
                var @ref = GetRef(refName, deref);
                if (!string.IsNullOrWhiteSpace(@ref.Value))
                {
                    yield return ValueTuple.Create(refName, @ref);
                }
                
            }
        }

        public Dictionary<string, string> GetIndex()
        {
            string path = _fileSystem.Path.Join(GitDir, "index");
            if (_fileSystem.File.Exists(path))
            {
                var content = _fileSystem.File.ReadAllBytes(path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(content);
            }
            return new Dictionary<string, string>();
        }

        public void SetIndex(Dictionary<string, string> index)
        {
            string path = _fileSystem.Path.Join(GitDir, "index");
            string content = JsonSerializer.Serialize(index);
            _fileSystem.File.WriteAllText(path, content);
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
            if (!_fileSystem.File.Exists(filePath))
            {
                return string.Empty.Encode();
            }
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