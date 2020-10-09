using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

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
                if (IsIgnore(filePath)) continue;
                Console.WriteLine(filePath);
            }
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                if (IsIgnore(directoryPath)) continue;
                WriteTree(directoryPath);
            }
        }

        private bool IsIgnore(string path) => path.Split(Path.DirectorySeparatorChar).Contains(dataProvider.GitDir);
    }
}
