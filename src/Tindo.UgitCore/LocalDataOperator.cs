using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore
{
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
    }
}
