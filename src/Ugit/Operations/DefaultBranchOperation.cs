namespace Ugit.Operations
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Default implementation of IBranchOperation.
    /// </summary>
    internal class DefaultBranchOperation : IBranchOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultBranchOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultBranchOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public IEnumerable<string> Names
        {
            get
            {
                foreach (var (refName, _) in this.dataProvider.IterRefs(Path.Join("refs", "heads")))
                {
                    yield return Path.GetRelativePath(Path.Join("refs", "heads"), refName);
                }
            }
        }

        /// <inheritdoc/>
        public string Current
        {
            get
            {
                var HEAD = this.dataProvider.GetRef("HEAD", false);
                if (!HEAD.Symbolic)
                {
                    return null;
                }

                var head = HEAD.Value;
                if (!head.StartsWith(Path.Join("refs", "heads")))
                {
                    throw new System.Exception("Branch ref should start with refs/heads");
                }

                return Path.GetRelativePath(Path.Join("refs", "heads"), head);
            }
        }

        /// <inheritdoc/>
        public void Create(string name, string oid)
        {
            string @ref = Path.Join("refs", "heads", name);
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, oid));
        }

        /// <inheritdoc/>
        public bool IsBranch(string branch)
        {
            string path = Path.Join("refs", "heads", branch);
            return !string.IsNullOrWhiteSpace(this.dataProvider.GetRef(path).Value);
        }
    }
}
