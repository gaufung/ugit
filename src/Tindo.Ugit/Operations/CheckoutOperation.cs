namespace Tindo.Ugit
{
    using System.IO;

    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Default implementation of Checkout operation.
    /// </summary>
    internal class CheckoutOperation : ICheckoutOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        private readonly ICommitOperation commitOperation;

        private readonly IBranchOperation branchOperation;

        private readonly ILogger<CheckoutOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="treeOperation">The tree opeartion.</param>
        /// <param name="commitOperation">The commit operation.</param>
        /// <param name="branchOperation">The branch operation.</param>
        public CheckoutOperation(
            IDataProvider dataProvider,
            ITreeOperation treeOperation,
            ICommitOperation commitOperation,
            IBranchOperation branchOperation)
            : this(dataProvider, treeOperation, commitOperation, branchOperation, NullLogger<CheckoutOperation>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CheckoutOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="treeOperation">The tree operation.</param>
        /// <param name="commitOperation">The commit operation.</param>
        /// <param name="branchOperation">The branch operation.</param>
        /// <param name="logger">The logger factory.</param>
        public CheckoutOperation(
            IDataProvider dataProvider,
            ITreeOperation treeOperation,
            ICommitOperation commitOperation,
            IBranchOperation branchOperation,
            ILogger<CheckoutOperation> logger)
        {
            this.dataProvider = dataProvider;
            this.treeOperation = treeOperation;
            this.commitOperation = commitOperation;
            this.branchOperation = branchOperation;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Checkout(string name)
        {
            string oid = this.dataProvider.GetOid(name);
            Commit commit = this.commitOperation.GetCommit(oid);
            this.treeOperation.ReadTree(commit.Tree, true);

            RefValue HEAD = this.branchOperation.IsBranch(name)
                ? RefValue.Create(true, Path.Join(Constants.Refs, Constants.Heads, name))
                : RefValue.Create(false, oid);
            this.dataProvider.UpdateRef(Constants.HEAD, HEAD, false);
        }
    }
}
