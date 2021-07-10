using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

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

        private readonly ILogger<TagOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TagOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public TagOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            this.logger = new NullLogger<TagOperation>();
        }

        public TagOperation(IDataProvider dataProvider, ILoggerFactory loggerFactory)
        : this(dataProvider)
        {
            this.logger = loggerFactory.CreateLogger<TagOperation>();
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
