﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Tindo.Ugit;

namespace Tindo.Ugit
{
    [TestClass]
    public class MergeOperationTests
    {
        private Mock<ICommitOperation> commitOperation;

        private Mock<IDataProvider> dataProvider;

        private Mock<IDiffOperation> diff;

        private Mock<ITreeOperation> treeOperation;

        private IMergeOperation mergeOperation;

        [TestInitialize]
        public void Init()
        {
            commitOperation = new Mock<ICommitOperation>();
            dataProvider = new Mock<IDataProvider>();
            diff = new Mock<IDiffOperation>();
            treeOperation = new Mock<ITreeOperation>();
            mergeOperation = new MergeOperation(dataProvider.Object,
                commitOperation.Object, treeOperation.Object, diff.Object);
        }

        [TestMethod]
        public void FastwardMergeTest()
        {
            dataProvider.Setup(d => d.GetRef("HEAD", true)).Returns(RefValue.Create(false, "head-tree-oid"));
            commitOperation.Setup(c => c.GetCommit("head-tree-oid")).Returns(new Commit
            {
                Tree = "head-tree-oid",
            });

            commitOperation.Setup(c => c.GetCommit("other-oid")).Returns(new Commit
            {
                Tree = "other-tree-oid",
            });

            commitOperation.Setup(c => c.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>((oids) =>
            {
                if (oids.ToArray()[0] == "other-oid")
                {
                    return new string[] { "other-oid", "head-tree-oid" };
                }
                if (oids.ToArray()[0] == "head-tree-oid")
                {
                    return new string[] { "head-tree-oid" };
                }
                return Array.Empty<string>();
            });

            this.treeOperation.Setup(t => t.ReadTree("other-tree-oid", true));
            this.dataProvider.Setup(d => d.UpdateRef("HEAD", It.Is<RefValue>(r => !r.Symbolic && r.Value == "other-oid"), true));
            mergeOperation.Merge("other-oid");
            dataProvider.VerifyAll();
            treeOperation.VerifyAll();
            commitOperation.VerifyAll();
        }

        [TestMethod]
        public void MergeTest()
        {
            dataProvider.Setup(d => d.GetRef("HEAD", true)).Returns(RefValue.Create(false, "head-tree-oid"));
            commitOperation.Setup(c => c.GetCommit("head-tree-oid")).Returns(new Commit
            {
                Tree = "head-tree-oid",
            });

            commitOperation.Setup(c => c.GetCommit("other-oid")).Returns(new Commit
            {
                Tree = "other-tree-oid",
            });

            commitOperation.Setup(c => c.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>((oids) =>
            {
                if (oids.ToArray()[0] == "other-oid")
                {
                    return new string[] { "other-oid", "parent-oid"};
                }
                if (oids.ToArray()[0] == "head-tree-oid")
                {
                    return new string[] { "head-tree-oid", "parent-oid" };
                }
                return Array.Empty<string>();
            });

            this.dataProvider.Setup(d => d.UpdateRef("MERGE_HEAD", It.Is<RefValue>(r => !r.Symbolic && r.Value == "other-oid"), true));
            this.dataProvider.Setup(d => d.Index).Returns(new Tree());
            this.treeOperation.Setup(d => d.GetTree("head-tree-oid", "")).Returns(
                new Tree()
                {
                    { "foo.txt", "foo-oid"},
                    { "bar.txt", "bar-oid" }
                });
            this.treeOperation.Setup(d => d.GetTree("other-tree-oid", "")).Returns(
                new Tree()
                {
                    { "foo.txt", "foo-oid"},
                });
            this.diff.Setup(d => d.MergeTree(It.IsAny<Tree>(),
                It.IsAny<Tree>())).Returns<Tree, Tree>((tree1, tree2) =>
                {
                    if(tree1.Count==2 && 
                        tree1.ContainsKey("foo.txt") &&
                        tree1.ContainsKey("bar.txt") &&
                        tree2.Count == 1 &&
                        tree2.ContainsKey("foo.txt"))
                    {
                        return new Tree()
                        {
                            { "foo.txt", "foo-oid" },
                            { "bar.txt", "bar-oid" }
                        };
                    }

                    throw new Exception();
                });
            mergeOperation.Merge("other-oid");
            dataProvider.VerifyAll();
            treeOperation.VerifyAll();
            commitOperation.VerifyAll();
            diff.VerifyAll();
        }

    }
}
