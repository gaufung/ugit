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

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultAddOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultAddOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public void Add(IEnumerable<string> fileNames)
        {
            var index = this.dataProvider.GetIndex();
            foreach (var name in fileNames)
            {
                if (this.dataProvider.Exist(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this.dataProvider.Exist(name, false))
                {
                    this.AddDirectionary(index, name);
                }
            }

            this.dataProvider.SetIndex(index);
        }

        private void AddDirectionary(IDictionary<string, string> index, string directoryName)
        {
            foreach (var fileName in this.dataProvider.Walk(directoryName))
            {
                this.AddFile(index, fileName);
            }
        }

        private void AddFile(IDictionary<string, string> index, string fileName)
        {
            if (!this.dataProvider.IsIgnore(fileName))
            {
                var normalFileName = Path.GetRelativePath(".", fileName);
                byte[] data = this.dataProvider.ReadAllBytes(normalFileName);
                string oid = this.dataProvider.HashObject(data);
                index[normalFileName] = oid;
            }
        }
    }
}
