namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;
    using System.IO;
    
    public class DefaultAddOperation : IAddOperation
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
                if (this.fileOperator.Exists(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this.fileOperator.Exists(name, false))
                {
                    this.AddDirectory(index, name);
                }
            }

            this.dataProvider.Index = index;
        }

        private void AddDirectory(Tree index, string directoryName)
        {
            foreach (var fileName in this.fileOperator.Walk(directoryName))
            {
                this.AddFile(index, fileName);
            }
        }

        private void AddFile(Tree index, string fileName)
        {
            if (this.dataProvider.IsIgnore(fileName))
            {
                return;
            }

            var normalFileName = Path.GetRelativePath(".", fileName);
            byte[] data = this.fileOperator.Read(normalFileName);
            string oid = this.dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }
    }
}