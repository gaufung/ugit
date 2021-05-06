﻿namespace Tindo.UgitCore.Operations
{
    using System.IO;

    /// <summary>
    /// Default implementation of Checkout operation.
    /// </summary>
    public class DefaultCheckoutOperation : ICheckoutOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        private readonly ICommitOperation commitOperation;

        private readonly IBranchOperation branchOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCheckoutOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="treeOperation">The tree operation.</param>
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
                ? RefValue.Create(true, Path.Join(Constants.Refs, Constants.Heads, name))
                : RefValue.Create(false, oid);
            this.dataProvider.UpdateRef(Constants.HEAD, HEAD, false);
        }
    }
}