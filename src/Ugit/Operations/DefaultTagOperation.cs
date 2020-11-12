namespace Ugit.Operations
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Default implementation of Tag operation.
    /// </summary>
    internal class DefaultTagOperation : ITagOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultTagOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultTagOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public IEnumerable<string> All
        {
            get
            {
                string prefix = Path.Join("refs", "tags");
                foreach (var (tagRef, _) in this.dataProvider.GetAllRefs(prefix, false))
                {
                    yield return Path.GetRelativePath(prefix, tagRef);
                }
            }
        }

        /// <inheritdoc/>
        public void Create(string name, string oid)
        {
            string @ref = Path.Join("refs", "tags", name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }
    }
}
