

using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore.Operations
{
    public class AddOperation : IAddOperation
    {
        private readonly IDataOperator _dataOperator;
        private readonly IFileOperator _fileOperator;

        private readonly ILogger<AddOperation> _logger;

        public AddOperation(IDataOperator dataOperator, IFileOperator fileOperator, ILoggerFactory loggerFactory)
        {
            this._dataOperator = dataOperator;
            this._fileOperator = fileOperator;
            this._logger = loggerFactory.CreateLogger<AddOperation>();
        }

        public void Add(IEnumerable<string> fileNames)
        {
            var index = this._dataOperator.Index;
            foreach (var name in fileNames)
            {
                if (this._fileOperator.Exists(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this._fileOperator.Exists(name, false))
                {
                    this.AddDirectory(index, name);
                }
            }

            this._dataOperator.Index = index;
        }
        
        private void AddDirectory(Tree index, string directoryName)
        {
            foreach (var fileName in this._fileOperator.Walk(directoryName))
            {
                this.AddFile(index, fileName);
            }
        }

        private void AddFile(Tree index, string fileName)
        {
            if (Utility.IsIgnore(fileName))
            {
                return;
            }

            var normalFileName = Path.GetRelativePath(".", fileName);
            this._fileOperator.TryRead(normalFileName, out var data);
            string oid = this._dataOperator.WriteObject(data);
            index[normalFileName] = oid;
        }
    }
}