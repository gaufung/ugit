namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Default implementation of Tag operation.
    /// </summary>
    internal class TagOperation : ITagOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public TagOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public IEnumerable<string> All
        {
            get
            {
                string prefix = Path.Join(Constants.Refs, Constants.Tags);
                foreach (var (tagRef, _) in this.dataProvider.GetAllRefs(prefix, false))
                {
                    yield return Path.GetRelativePath(prefix, tagRef);
                }
            }
        }

        /// <inheritdoc/>
        public void Create(string name, string oid)
        {
            string @ref = Path.Join(Constants.Refs, Constants.Tags, name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }
    }
}
