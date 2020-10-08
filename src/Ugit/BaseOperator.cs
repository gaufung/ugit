using System;
using System.IO.Abstractions;

namespace Ugit
{
    internal class BaseOperator : IBaseOperator
    {
        private readonly IFileSystem fileSystem;

        private readonly IDataProvider dataProvider;

        public BaseOperator(IFileSystem fileSystem, IDataProvider dataprovider)
        {
            this.fileSystem = fileSystem;
            this.dataProvider = dataprovider;
        }

        public void WriteTree(string directory = ".")
        {
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                Console.WriteLine(filePath);
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                WriteTree(directoryPath);
            }
        }
    }
}
