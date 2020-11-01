namespace Ugit.Operations
{
    using System.IO;

    /// <summary>
    /// Default implementation of Checkout operation.
    /// </summary>
    internal class DefaultCheckoutOperation : ICheckoutOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        private readonly ICommitOperation commitOperation;

        private readonly IBranchOperation branchOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCheckoutOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="treeOperation">The tree opeartion.</param>
        /// <param name="commitOperation">The commit operation.</param>
        /// <param name="branchOperation">The branch operation.</param>
        public DefaultCheckoutOperation(
            IDataProvider dataProvider,
            ITreeOperation treeOperation,
            ICommitOperation commitOperation,
            IBranchOperation branchOperation)
        {
            this.dataProvider = dataProvider;
            this.treeOperation = treeOperation;
            this.commitOperation = commitOperation;
            this.branchOperation = branchOperation;
        }

        /// <inheritdoc/>
        public void Checkout(string name)
        {
            string oid = this.dataProvider.GetOid(name);
            Commit commit = this.commitOperation.GetCommit(oid);
            this.treeOperation.ReadTree(commit.Tree, true);

            RefValue HEAD = this.branchOperation.IsBranch(name)
                ? RefValue.Create(true, Path.Join("refs", "heads", name))
                : RefValue.Create(false, oid);
            this.dataProvider.UpdateRef("HEAD", HEAD, false);
        }
    }
}
