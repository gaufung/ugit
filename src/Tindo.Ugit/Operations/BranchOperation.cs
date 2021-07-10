using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// Default implementation of IBranchOperation.
    /// </summary>
    internal class BranchOperation : IBranchOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ILogger<BranchOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="BranchOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public BranchOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
            this.logger = new NullLogger<BranchOperation>();
        }

        public BranchOperation(IDataProvider dataProvider, ILoggerFactory loggerFactory)
         : this(dataProvider)
        {
            this.logger = loggerFactory.CreateLogger<BranchOperation>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> Names
        {
            get
            {
                foreach (var (refName, _) in this.dataProvider.GetAllRefs(Path.Join(Constants.Refs, Constants.Heads)))
                {
                    yield return Path.GetRelativePath(Path.Join(Constants.Refs, Constants.Heads), refName);
                }
            }
        }

        /// <inheritdoc/>
        public string Current
        {
            get
            {
                var HEAD = this.dataProvider.GetRef(Constants.HEAD, false);
                if (!HEAD.Symbolic)
                {
                    return null;
                }

                var head = HEAD.Value;
                if (!head.StartsWith(Path.Join(Constants.Refs, Constants.Heads)))
                {
                    throw new UgitException("Branch ref should start with refs/heads");
                }

                return Path.GetRelativePath(Path.Join(Constants.Refs, Constants.Heads), head);
            }
        }

        /// <inheritdoc/>
        public void Create(string name, string oid)
        {
            string @ref = Path.Join(Constants.Refs, Constants.Heads, name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        /// <inheritdoc/>
        public bool IsBranch(string branch)
        {
            string path = Path.Join(Constants.Refs, Constants.Heads, branch);
            return !string.IsNullOrWhiteSpace(this.dataProvider.GetRef(path).Value);
        }
    }
}
