namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Default implementation of Tag operation.
    /// </summary>
    internal class TagOperation : ITagOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ILogger<TagOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public TagOperation(IDataProvider dataProvider)
            : this(dataProvider, NullLogger<TagOperation>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TagOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="logger">The logger.</param>
        public TagOperation(IDataProvider dataProvider, ILogger<TagOperation> logger)
        {
            this.dataProvider = dataProvider;
            this.logger = logger;
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
