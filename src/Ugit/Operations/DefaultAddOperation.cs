namespace Ugit.Operations
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Default implementation of <seealso cref="IAddOperation"/>.
    /// </summary>
    internal class DefaultAddOperation : IAddOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAddOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultAddOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public void AddDirectionary(IDictionary<string, string> index, string directoryName)
        {
            foreach (var fileName in this.dataProvider.FileSystem.Walk(directoryName))
            {
                if (this.dataProvider.IsIgnore(fileName))
                {
                    this.AddDirectionary(index, fileName);
                }
            }
        }

        /// <inheritdoc/>
        public void AddFile(IDictionary<string, string> index, string fileName)
        {
            var normalFileName = Path.GetRelativePath(".", fileName);
            byte[] data = this.dataProvider.FileSystem.File.ReadAllBytes(normalFileName);
            string oid = this.dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }
    }
}
