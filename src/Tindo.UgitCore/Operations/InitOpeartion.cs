namespace Tindo.UgitCore.Operations
{
    using Microsoft.Extensions.Logging;
    using System;
    using System.IO;

    public class InitOpeartion : IInitOperation
    {
        private IDataOperator dataOperator;

        private ILogger logger;

        public InitOpeartion(IDataOperator dataOperator, ILoggerFactory logFactory)
        {
            this.dataOperator = dataOperator;
            this.logger = logFactory.CreateLogger<InitOpeartion>();
        }

        public void Initialize()
        {
            this.dataOperator.Initialize();
            this.logger.LogInformation($"initilize ugit repository in {this.dataOperator.RepositoryPath}");
            this.dataOperator.UpdateRef(Constants.HEAD, RefValue.Create(true, Path.Join(Constants.Refs, Constants.Heads, Constants.Master)));
            Console.WriteLine($"Initialized empty Ugit repository in {this.dataOperator.RepositoryPath}");
        }
    }
}
