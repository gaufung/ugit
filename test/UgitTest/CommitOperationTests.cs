using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Tindo.Ugit;

namespace Tindo.Ugit
{
    [TestClass]
    public class CommitOperationTests
    {
        private Mock<IDataProvider> dataProvider;
        private Mock<ITreeOperation> treeOperation;

        private ICommitOperation commitOperation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            treeOperation = new Mock<ITreeOperation>();
            commitOperation = new CommitOperation(dataProvider.Object, treeOperation.Object);
        }

        [TestMethod]
        public void CreateCommitTest()
        {
            this.treeOperation.Setup(t => t.WriteTree()).Returns("tree-oid");
            this.dataProvider.Setup(d => d.GetRef("HEAD", true)).Returns(RefValue.Create(false, "master-first-oid"));
            this.dataProvider.Setup(d => d.GetObject("master-first-oid", "commit")).Returns(string.Join("\n", new[]
            {
                "tree master-tree-oid",
                "author unknown",
                "",
                "this is message."

            }).Encode());
            this.treeOperation.Setup(d => d.GetTree("master-tree-oid", "")).Returns(new Tree()
            {
                { "hello.txt", "hello.oid"}
            });
            this.treeOperation.Setup(d => d.GetIndexTree()).Returns(new Tree()
            {
                {"hello.txt", "hello.oid" },
                {"ugit.txt", "ugit.oid" }
            });
            this.dataProvider.Setup(d => d.GetRef("MERGE_HEAD", true)).Returns(RefValue.Create(false, "merge-head-oid"));
            this.dataProvider.Setup(d => d.DeleteRef("MERGE_HEAD", false));
            string commit = "tree tree-oid\nparent master-first-oid\nparent merge-head-oid\nauthor unknown\n\nhello foo\n";
            this.dataProvider.Setup(d => d.WriteObject(It.Is<byte[]>(i => i.Length == commit.Encode().Length), "commit")).Returns("commit-oid");
            dataProvider.Setup(d => d.UpdateRef("HEAD", It.Is<RefValue>(i => !i.Symbolic && i.Value == "commit-oid"), true));
            string actual = commitOperation.CreateCommit("hello foo");
            Assert.AreEqual("commit-oid", actual);
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void CreateCommitExceptionTest()
        {
            this.treeOperation.Setup(t => t.WriteTree()).Returns("tree-oid");
            this.dataProvider.Setup(d => d.GetRef("HEAD", true)).Returns(RefValue.Create(false, ""));
            this.treeOperation.Setup(d => d.GetTree("", "")).Returns(new Tree()
            {
            });
            this.treeOperation.Setup(d => d.GetIndexTree()).Returns(new Tree()
            {
            });
            _ = commitOperation.CreateCommit("hello foo");
        }

        [TestMethod]
        public void GetCommitTest()
        {
            string commitMessage = string.Join("\n", new[]
            {
                "tree tree-oid",
                "parent parent-oid",
                "parent merge-parent-oid",
                "",
                "this is ugit commit",
                "related workitem #1"
            });

            this.dataProvider.Setup(d => d.GetObject("foo-oid", "commit")).Returns(commitMessage.Encode());
            var commit = commitOperation.GetCommit("foo-oid");
            Assert.AreEqual("tree-oid", commit.Tree);
            CollectionAssert.AreEqual(new string[] { "parent-oid", "merge-parent-oid" },
                commit.Parents);
            Assert.AreEqual("this is ugit commit\nrelated workitem #1", commit.Message);
            dataProvider.VerifyAll();
        }

        [TestMethod]
        public void CommitHistoryTest()
        {
            string commitMessage1 = string.Join("\n", new[]
            {
                "tree tree-oid",
                "parent parent-oid",
                "",
                "this is second commit",
            });

            string commitMessage2 = string.Join("\n", new[]
            {
                "tree tree-oid",
                "",
                "this is first commit",
            });

            this.dataProvider.Setup(d => d.GetObject("foo-oid", "commit")).Returns(commitMessage1.Encode());
            this.dataProvider.Setup(d => d.GetObject("parent-oid", "commit")).Returns(
                commitMessage2.Encode());
            var history = commitOperation.GetCommitHistory(new string[] { "foo-oid" });
            CollectionAssert.AreEqual(new string[] { "foo-oid", "parent-oid" }, history.ToArray());
            dataProvider.VerifyAll();
        }

        [TestMethod]
        public void GetObjectHistroyTest()
        {
            string commitMessage1 = string.Join("\n", new[]
            {
                "tree tree2-oid",
                "parent parent-oid",
                "",
                "this is second commit",
            });

            string commitMessage2 = string.Join("\n", new[]
            {
                "tree tree1-oid",
                "",
                "this is first commit",
            });

            this.dataProvider.Setup(d => d.GetObject("foo-oid", "commit")).Returns(commitMessage1.Encode());
            this.dataProvider.Setup(d => d.GetObject("parent-oid", "commit")).Returns(
                commitMessage2.Encode());
            this.treeOperation.Setup(t => t.IterTreeEntry(It.IsAny<string>())).Returns<string>((oid)=>{
                if (oid == "tree2-oid")
                {
                    return new []{
                        ("tree", "sub-tree-oid", ""),
                        ("blob", "foo.txt-oid", ""),
                    };
                }
                if (oid == "sub-tree-oid")
                {
                    return new []{
                        ("blob", "bar.txt-oid", "")
                    };
                }
                if (oid == "tree1-oid")
                {
                    return new [] {
                         ("blob", "bar.txt-oid", "")
                    };
                }
                throw new System.Exception($"Unknow oid: {oid}");
            });
            string[] objects = commitOperation.GetObjectHistory(new []{ "foo-oid"}).ToArray();
            CollectionAssert.AreEqual(new string[]{
                "foo-oid", "tree2-oid", "sub-tree-oid", "bar.txt-oid", "foo.txt-oid", "parent-oid", "tree1-oid", "bar.txt-oid"},
                 objects);
            treeOperation.VerifyAll();
        }
    }
}
