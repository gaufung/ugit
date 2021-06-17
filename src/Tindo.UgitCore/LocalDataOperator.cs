namespace Tindo.UgitCore
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
    using System.Linq;

    public class LocalDataOperator : IDataOperator
    {
        private readonly string repoRootPath;

        private readonly ILogger<LocalDataOperator> logger;

        private readonly string repoUgitPath;

        private readonly IFileOperator localFileOperator;

        public LocalDataOperator(IFileOperator localFileOperator, string repoRootPath, ILoggerFactory loggerFactory)
        {
            this.logger = loggerFactory.CreateLogger<LocalDataOperator>();
            this.localFileOperator = localFileOperator;
            if (string.IsNullOrEmpty(repoRootPath))
            {
                repoRootPath = this.localFileOperator.CurrentDirectory;
            }

            this.repoRootPath = repoRootPath;
            this.repoUgitPath = Path.Join(this.repoRootPath, Constants.GitDir);
            this.logger.LogInformation($"Initilize {nameof(LocalDataOperator)} within {this.repoRootPath}");
        }

        public LocalDataOperator(IFileOperator localFileOpeator, ILoggerFactory loggerFactory)
            : this (localFileOpeator, "", loggerFactory)
        {

        }

        public byte[] GetObject(string oid, string expected = "blob")
        {
            string objectFilePath = Path.Join(this.repoUgitPath, Constants.Objects, oid);
            if (!this.localFileOperator.TryRead(objectFilePath, out var data))
            {
                return Array.Empty<byte>();
            }

            var index = Array.IndexOf(data, Constants.TypeSeparator);
            if (index < 0)
            {
                return Array.Empty<byte>();
            }

            var actualType = data.Take(index).ToArray().Decode();
            if (!string.Equals(expected, actualType, StringComparison.OrdinalIgnoreCase))
            {
                this.logger.LogError($"Failed to read object {oid}, expect {expected} but got {actualType}");
                throw new UgitException($"Failed to read object {oid}, expect {expected} but got {actualType}");
            }

            return data.TakeLast(data.Length - index - 1).ToArray();
        }

        public string WriteObject(byte[] data, string type="blob")
        {
            if (!string.IsNullOrEmpty(type))
            {
                data = type.Encode().Concat(new byte[] { Constants.TypeSeparator }).Concat(data).ToArray();
            }

            string oid = data.Sha1HexDigest();
            string objectFilePath = Path.Join(this.repoUgitPath, Constants.Objects, oid);
            this.localFileOperator.Write(objectFilePath, data);
            return oid;
        }

        public void Initialize()
        {
            this.localFileOperator.CreateDirectory(this.repoUgitPath);
            string objectDirectory = Path.Join(this.repoUgitPath, Constants.Objects);
            this.localFileOperator.CreateDirectory(objectDirectory);
        }
    }
}
