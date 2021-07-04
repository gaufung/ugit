using System;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Tindo.Ugit
{
    internal class PhysicalFileOperator : IFileOperator
    {
        private readonly IFileSystem _fileSystem;

        public string CurrentDirectory => this._fileSystem.Directory.GetCurrentDirectory();

        public PhysicalFileOperator(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public void Delete(string path, bool isFile = true)
        {
            if (isFile && this._fileSystem.File.Exists(path))
            {
                this._fileSystem.File.Delete(path);
            }
            else if (!isFile && this._fileSystem.Directory.Exists(path))
            {
                this._fileSystem.Directory.Delete(path, true);
            }
        }

        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            foreach (var filePath in this._fileSystem.Directory.EnumerateFiles("."))
            {
                if (ignore(filePath)) continue;
                this.Delete(filePath);
            }
            foreach (var directory in this._fileSystem.Directory.EnumerateDirectories("."))
            {
                if (ignore(directory)) continue;
                this.Delete(directory, false);
            }
        }

        public bool Exists(string path, bool isFile = true)
        {
            return isFile ? this._fileSystem.File.Exists(path)
               : this._fileSystem.Directory.Exists(path);
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            if (this.Exists(path))
            {
                bytes = this._fileSystem.File.ReadAllBytes(path);
                return true;
            }

            bytes = Array.Empty<byte>();
            return false;
        }

        public IEnumerable<string> Walk(string path)
{
            if (!this.Exists(path, false))
            {
                yield break;
}

            foreach (var filePath in this._fileSystem.Directory.EnumerateFiles(path))
            {
                yield return filePath;
            }
            foreach (var subDirectory in this._fileSystem.Directory.EnumerateDirectories(path))
            {
                foreach (var filepath in this.Walk(subDirectory))
                {
                    yield return filepath;
                }
            }
        }

        public void Write(string path, byte[] bytes)
        {
            CreateParentDirectory(path);
            if (this.Exists(path))
            {
                this.Delete(path);
            }

            this._fileSystem.File.WriteAllBytes(path, bytes);
        }

        private void CreateParentDirectory(string filePath)
        {
            string parentDirectory = this._fileSystem.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(parentDirectory) && !this._fileSystem.Directory.Exists(parentDirectory))
            {
                this._fileSystem.Directory.CreateDirectory(parentDirectory);
            }
        }

        public void CreateDirectory(string directory, bool force = true)
        {
            if (this._fileSystem.Directory.Exists(directory) && !force)
            {
                throw new UgitException($"{directory} is not empty.");
            }
            this._fileSystem.Directory.CreateDirectory(directory);
        }

        public byte[] Read(string path)
        {
            if (this.TryRead(path, out var data))
            {
                return data;
            }
            throw new UgitException("file doesn't exist");
        }
    }
}
