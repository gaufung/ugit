namespace Ugit.Operations
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Default implementation of <seealso cref="IAddOperation"/>.
    /// </summary>
    internal class DefaultAddOperation : IAddOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly IFileOperator fileOperator;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAddOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultAddOperation(IDataProvider dataProvider, IFileOperator fileOperator)
        {
            this.dataProvider = dataProvider;
            this.fileOperator = fileOperator;
        }

        /// <inheritdoc/>
        public void Add(IEnumerable<string> fileNames)
        {
            var index = this.dataProvider.Index;
            foreach (var name in fileNames)
            {
                if (this.fileOperator.Exist(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this.fileOperator.Exist(name, false))
                {
                    this.AddDirectionary(index, name);
                }
            }

            this.dataProvider.Index = index;
        }

        private void AddDirectionary(IDictionary<string, string> index, string directoryName)
        {
            foreach (var fileName in this.fileOperator.Walk(directoryName))
            {
                this.AddFile(index, fileName);
            }
        }

        private void AddFile(IDictionary<string, string> index, string fileName)
        {
            if (!this.dataProvider.IsIgnore(fileName))
            {
                var normalFileName = Path.GetRelativePath(".", fileName);
                byte[] data = this.fileOperator.Read(normalFileName);
                string oid = this.dataProvider.HashObject(data);
                index[normalFileName] = oid;
            }
        }
    }
}
