using System;

namespace Tindo.UgitCore
{
    using System.IO.Abstractions;

    public class PhysicalFileOperator : IFileOperator
    {
        private readonly IFileSystem fileSystem;

        public PhysicalFileOperator(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public string CurrentDirectory => this.fileSystem.Directory.GetCurrentDirectory();

        public void CreateDirectory(string directory, bool force = true)
        {
            if (this.fileSystem.Directory.Exists(directory) && !force)
            {
                throw new UgitException($"{directory} is not empty.");
            }
            this.fileSystem.Directory.CreateDirectory(directory);
        }

        public void Delete(string path, bool isFile = true)
        {
            if (isFile && this.fileSystem.File.Exists(path))
            {
                this.fileSystem.File.Delete(path);
            }
            else if (!isFile && this.fileSystem.Directory.Exists(path))
            {
                this.fileSystem.Directory.Delete(path);
            }
        }

        public bool Exists(string path, bool isFile = true)
        {
            return isFile ? this.fileSystem.File.Exists(path)
                : this.fileSystem.Directory.Exists(path);
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            if (this.Exists(path))
            {
                bytes = this.fileSystem.File.ReadAllBytes(path);
                return true;
            }
            bytes = Array.Empty<byte>();
            return false;
        }

        public void Write(string filePath, byte[] data)
        {
            CreateDirectory(filePath);
            if (this.Exists(filePath))
            {
                this.Delete(filePath);
            }

            this.fileSystem.File.WriteAllBytes(filePath, data);
        }

        private void CreateParentDirectory(string filePath)
        {
            string parentDirectory = this.fileSystem.Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(parentDirectory) && !this.fileSystem.Directory.Exists(parentDirectory))
            {
                this.fileSystem.Directory.CreateDirectory(parentDirectory);
            }
        }
    }
}