namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;

    /// <inheritdoc />
    internal class PhysicalFileOperator : IFileOperator
    {
        private readonly IFileSystem fileSystem;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicalFileOperator"/> class.
        /// </summary>
        /// <param name="fileSystem">The File system.</param>
        public PhysicalFileOperator(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        /// <inheritdoc/>
        public string CurrentDirectory => this.fileSystem.Directory.GetCurrentDirectory();

        /// <inheritdoc/>
        public void Delete(string path, bool isFile = true)
        {
            if (isFile && this.fileSystem.File.Exists(path))
            {
                this.fileSystem.File.Delete(path);
            }
            else if (!isFile && this.fileSystem.Directory.Exists(path))
            {
                this.fileSystem.Directory.Delete(path, true);
            }
        }

        /// <inheritdoc/>
        public void CleanDirectory(string directory, Func<string, bool> ignore)
        {
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = ".";
            }

            foreach (var filePath in this.fileSystem.Directory.EnumerateFiles(directory))
            {
                if (ignore(filePath))
                {
                    continue;
                }

                this.Delete(filePath);
            }

            foreach (var dir in this.fileSystem.Directory.EnumerateDirectories(directory))
            {
                if (ignore(dir))
                {
                    continue;
                }

                this.Delete(dir, false);
            }
        }

        /// <inheritdoc/>
        public bool Exists(string path, bool isFile = true)
        {
            return isFile ? this.fileSystem.File.Exists(path)
               : this.fileSystem.Directory.Exists(path);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public IEnumerable<string> Walk(string path)
        {
            if (!this.Exists(path, false))
            {
                yield break;
            }

            foreach (var filePath in this.fileSystem.Directory.EnumerateFiles(path))
            {
                yield return filePath;
            }

            foreach (var subDirectory in this.fileSystem.Directory.EnumerateDirectories(path))
            {
                foreach (var filepath in this.Walk(subDirectory))
                {
                    yield return filepath;
                }
            }
        }

        /// <inheritdoc/>
        public void Write(string path, byte[] bytes)
        {
            this.CreateParentDirectory(path);
            if (this.Exists(path))
            {
                this.Delete(path);
            }

            this.fileSystem.File.WriteAllBytes(path, bytes);
        }

        /// <inheritdoc/>
        public void CreateDirectory(string directory, bool force = true)
        {
            if (this.fileSystem.Directory.Exists(directory) && !force)
            {
                throw new UgitException($"{directory} is not empty.");
            }

            this.fileSystem.Directory.CreateDirectory(directory);
        }

        /// <inheritdoc/>
        public byte[] Read(string path)
        {
            if (this.TryRead(path, out var data))
            {
                return data;
            }

            throw new UgitException("file doesn't exist");
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
