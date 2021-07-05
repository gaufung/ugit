﻿namespace Tindo.Ugit.Operations
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
        public DefaultAddOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            this.fileOperator = this.dataProvider.FileOperator;
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
