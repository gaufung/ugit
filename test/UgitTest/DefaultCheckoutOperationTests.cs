using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ugit.Operations;

namespace Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultCheckoutOperationTests
    {

        private Mock<IDataProvider> dataProvider;

        private Mock<ITreeOperation> treeOperation;

        private Mock<ICommitOperation> commitOperation;

        private Mock<IBranchOperation> branchOperation;

        private ICheckoutOperation checkoutOperation;
       
        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            treeOperation = new Mock<ITreeOperation>();
            commitOperation = new Mock<ICommitOperation>();
            branchOperation = new Mock<IBranchOperation>();

            checkoutOperation = new DefaultCheckoutOperation(dataProvider.Object,
                treeOperation.Object, commitOperation.Object, branchOperation.Object);
        }

        [TestMethod]
        public void ChecoutBranchTest()
        {
            dataProvider.Setup(d => d.GetOid("dev")).Returns("dev-oid");
            commitOperation.Setup(c => c.GetCommit("dev-oid")).Returns(new Commit
            {
                Tree = "dev-tree-oid"
            });
            treeOperation.Setup(t => t.ReadTree("dev-tree-oid", true));
            branchOperation.Setup(b => b.IsBranch("dev")).Returns(true);
            this.dataProvider.Setup(d => d.UpdateRef("HEAD", It.Is<RefValue>(r => r.Symbolic && r.Value == Path.Join("refs", "heads", "dev")), false));
            checkoutOperation.Checkout("dev");
            dataProvider.VerifyAll();
            commitOperation.VerifyAll();
            branchOperation.VerifyAll();
            treeOperation.VerifyAll();
        }

        [TestMethod]
        public void CheckoutOidTest()
        {
            dataProvider.Setup(d => d.GetOid("feature-oid")).Returns("feature-oid");
            commitOperation.Setup(c => c.GetCommit("feature-oid")).Returns(new Commit
            {
                Tree = "feature-tree-oid"
            });
            treeOperation.Setup(t => t.ReadTree("feature-tree-oid", true));
            branchOperation.Setup(b => b.IsBranch("feature-oid")).Returns(false);
            this.dataProvider.Setup(d => d.UpdateRef("HEAD", It.Is<RefValue>(r => !r.Symbolic && r.Value == "feature-oid"), false));
            checkoutOperation.Checkout("feature-oid");
            dataProvider.VerifyAll();
            commitOperation.VerifyAll();
            branchOperation.VerifyAll();
            treeOperation.VerifyAll();
        }
    }
}
