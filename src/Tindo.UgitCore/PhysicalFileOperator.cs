namespace Tindo.UgitCore
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;

    public class PhysicalFileOperator : IFileOperator
    {
        private readonly IFileSystem fileSystem;

        public PhysicalFileOperator(IFileSystem fileSystem)
        {
            this.fileSystem = fileSystem;
        }

        public string CurrentDirectory => this.fileSystem.Directory.GetCurrentDirectory();

        public void CreateDirectory(string directory)
        {
            this.fileSystem.Directory.CreateDirectory(directory);
        }

        public void Delete(string path)
        {
            if (this.Exists(path))
            {
                this.fileSystem.File.Delete(path);
            }

            if (this.Exists(path, false))
            {
                this.fileSystem.Directory.Delete(path);
            }
        }

        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            foreach (var filePath in this.fileSystem.Directory.EnumerateFiles("."))
            {
                if (ignore(filePath))
                {
                    continue;
                }

                this.Delete(filePath);
            }

            foreach (var directoryPath in this.fileSystem.Directory.EnumerateDirectories("."))
            {
                if (ignore(directoryPath))
                {
                    continue;
                }

                this.Delete(directoryPath);
            }
        }

        public bool Exists(string path, bool isFile = true)
        {
            return isFile ?
                this.fileSystem.File.Exists(path) :
                this.fileSystem.Directory.Exists(path);
        }

        public byte[] Read(string path)
        {
            return this.fileSystem.File.ReadAllBytes(path);
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            if (this.Exists(path, true))
            {
                bytes = this.Read(path);
                return true;
            }

            bytes = Array.Empty<byte>();
            return false;
        }

        public IEnumerable<string> Walk(string path)
        {
            return this.fileSystem.Walk(path);
        }

        public void Write(string path, byte[] bytes)
        {
            this.fileSystem.CreateParentDirectory(path);
            if (this.Exists(path))
            {
                this.Delete(path);
            }
            this.fileSystem.File.WriteAllBytes(path, bytes);
        }

    }
}
