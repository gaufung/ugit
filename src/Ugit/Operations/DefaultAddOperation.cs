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

        public void Add(IEnumerable<string> fileNames)
        {
            var index = this.dataProvider.GetIndex();

            foreach (var name in fileNames)
            {
                if (this.dataProvider.FileSystem.File.Exists(name))
                {
                    this.AddFile(index, name);
                }
                else if (this.dataProvider.FileSystem.Directory.Exists(name))
                {
                    this.AddDirectionary(index, name);
                }
            }

            this.dataProvider.SetIndex(index);
        }

        private void AddDirectionary(IDictionary<string, string> index, string directoryName)
        {
            foreach (var fileName in this.dataProvider.FileSystem.Walk(directoryName))
            {
                if (this.dataProvider.IsIgnore(fileName))
                {
                    this.AddDirectionary(index, fileName);
                }
            }
        }

        private void AddFile(IDictionary<string, string> index, string fileName)
        {
            var normalFileName = Path.GetRelativePath(".", fileName);
            byte[] data = this.dataProvider.FileSystem.File.ReadAllBytes(normalFileName);
            string oid = this.dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }
    }
}
