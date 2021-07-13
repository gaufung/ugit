namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Default implementation of <seealso cref="IAddOperation"/>.
    /// </summary>
    internal class AddOperation : IAddOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly IFileOperator fileOperator;

        private readonly ILogger<AddOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public AddOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            this.fileOperator = this.dataProvider.FileOperator;
            this.logger = NullLogger<AddOperation>.Instance;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AddOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="loggerFactory">loggerFactory interface.</param>
        public AddOperation(IDataProvider dataProvider, ILoggerFactory loggerFactory)
            : this(dataProvider)
        {
            this.logger = loggerFactory.CreateLogger<AddOperation>();
        }

        /// <inheritdoc/>
        public void Add(IEnumerable<string> fileNames)
        {
            var index = this.dataProvider.Index;
            foreach (var name in fileNames)
            {
                if (this.fileOperator.Exists(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this.fileOperator.Exists(name, false))
                {
                    this.AddDictionary(index, name);
                }
            }

            this.dataProvider.Index = index;
        }

        private void AddDictionary(Tree index, string directoryName)
        {
            foreach (var fileName in this.fileOperator.Walk(directoryName))
            {
                this.AddFile(index, fileName);
            }
        }

        private void AddFile(Tree tree, string fileName)
        {
            if (!this.dataProvider.IsIgnore(fileName))
            {
                var normalFileName = Path.GetRelativePath(".", fileName);
                byte[] data = this.fileOperator.Read(normalFileName);
                string oid = this.dataProvider.WriteObject(data);
                tree[normalFileName] = oid;
            }
        }
    }
}
