using System;
using System.Collections.Generic;

namespace Tindo.UgitCore
{
    using System.IO.Abstractions;

    public class PhysicalFileOperator : IFileOperator
    {
        private readonly IFileSystem _fileSystem;

        public PhysicalFileOperator(IFileSystem fileSystem)
        {
            this._fileSystem = fileSystem;
        }

        public string CurrentDirectory => this._fileSystem.Directory.GetCurrentDirectory();

        public void CreateDirectory(string directory, bool force = true)
        {
            if (this._fileSystem.Directory.Exists(directory) && !force)
            {
                throw new UgitException($"{directory} is not empty.");
            }
            this._fileSystem.Directory.CreateDirectory(directory);
        }

        public IEnumerable<string> Walk(string directory)
        {
            if (!this.Exists(directory, false))
            {
                yield break;
            }

            foreach (var filePath in this._fileSystem.Directory.EnumerateFiles(directory))
            {
                yield return filePath;
            }

            foreach (var subDirectory in this._fileSystem.Directory.EnumerateDirectories(directory))
            {
                foreach (var filepath in this.Walk(subDirectory))
                {
                    yield return filepath;
                }
            }
        }

        public void Delete(string path, bool isFile = true)
        {
            if (isFile && this._fileSystem.File.Exists(path))
            {
                this._fileSystem.File.Delete(path);
            }
            else if (!isFile && this._fileSystem.Directory.Exists(path))
            {
                this._fileSystem.Directory.Delete(path);
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

        public void Write(string filePath, byte[] data)
        {
            CreateParentDirectory(filePath);
            if (this.Exists(filePath))
            {
                this.Delete(filePath);
            }

            this._fileSystem.File.WriteAllBytes(filePath, data);
        }

        private void CreateParentDirectory(string filePath)
        {
            string parentDirectory = this._fileSystem.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(parentDirectory) && !this._fileSystem.Directory.Exists(parentDirectory))
            {
                this._fileSystem.Directory.CreateDirectory(parentDirectory);
            }
        }

        public void EmptyDirectory(Func<string, bool> ignore)
        {
            foreach(var filePath in this._fileSystem.Directory.EnumerateFiles("."))
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
    }
}