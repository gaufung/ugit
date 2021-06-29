using System.IO;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore.Operations
{
    public class CheckoutOperation : ICheckoutOperation
    {
        private readonly IDataOperator dataOperator;
        private readonly ITreeOperation treeOperation;
        private readonly ICommitOperation commitOperation;
        private readonly IBranchOperation branchOperation;
        private readonly ILogger<CheckoutOperation> logger;

        public CheckoutOperation(IDataOperator dataOperator,
            ITreeOperation treeOperation, ICommitOperation commitOperation,
            IBranchOperation branchOperation, ILoggerFactory loggerFactory)
        {
            this.dataOperator = dataOperator;
            this.treeOperation = treeOperation;
            this.commitOperation = commitOperation;
            this.branchOperation = branchOperation;
            this.logger = loggerFactory.CreateLogger<CheckoutOperation>();
        }

        public void Checkout(string name)
        {
            string oid = this.dataOperator.GetOid(name);
            Commit commit = this.commitOperation.Get(oid);
            this.treeOperation.Read(commit.Tree, true);
            RefValue HEAD = this.branchOperation.IsBranch(name)
               ? RefValue.Create(true, Path.Join(Constants.Refs, Constants.HEAD, name))
               : RefValue.Create(false, oid);
            this.dataOperator.UpdateRef(Constants.HEAD, HEAD, false);

        }
    }
}
