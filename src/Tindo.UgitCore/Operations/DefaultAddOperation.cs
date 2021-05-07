namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;
    using System.IO;
    
    public class DefaultAddOperation : IAddOperation
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
            var index = this.dataProvider.Index;
            foreach (var name in fileNames)
            {
                if (this.dataProvider.Exist(name, true))
                {
                    this.AddFile(index, name);
                }
                else if (this.dataProvider.Exist(name, false))
                {
                    this.AddDirectory(index, name);
                }
            }

            this.dataProvider.Index = index;
        }

        private void AddDirectory(Tree index, string directoryName)
        {
            foreach (var fileName in this.dataProvider.Walk(directoryName))
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
            byte[] data = this.dataProvider.Read(normalFileName);
            string oid = this.dataProvider.HashObject(data);
            index[normalFileName] = oid;
        }
    }
}