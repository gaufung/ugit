using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using Tindo.Ugit.Operations;

namespace Tindo.Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultTreeOperationTests
    {
        private Mock<IDataProvider> dataProvider;

        private Mock<IFileOperator> fileOperator;

        private ITreeOperation treeOpeartion;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            treeOpeartion = new DefaultTreeOperation(dataProvider.Object, fileOperator.Object);
        }

        /// <summary>
        /// Tree will like that 
        /// - foo
        /// --- a.txt
        /// --- bar
        /// ----- b.txt
        /// ----- c.md
        /// </summary>
        [TestMethod]
        public void GetTreeTest()
        {
            dataProvider.Setup(d => d.GetObject("foo-oid", "tree")).Returns(string.Join("\n", new []
            {
                "blob a-oid a.txt",
                "tree bar-oid bar"
            }).Encode());

            dataProvider.Setup(d => d.GetObject("bar-oid", "tree")).Returns(string.Join("\n", new[]
            {
                "blob b-oid b.txt",
                "blob c-oid c.md"
            }).Encode());

            var tree = treeOpeartion.GetTree("foo-oid", "");
            Assert.AreEqual(3, tree.Count);
            Assert.IsTrue(tree.ContainsKey(Path.Join("a.txt")));
            Assert.IsTrue(tree.ContainsKey(Path.Join("bar", "b.txt")));
            Assert.IsTrue(tree.ContainsKey(Path.Join("bar", "c.md")));
        }

        [TestMethod]
        public void CheckoutIndexTest()
        {
            fileOperator.Setup(d => d.EmptyCurrentDirectory(this.dataProvider.Object.IsIgnore));
            dataProvider.Setup(d => d.GetObject("foo-oid", "blob")).Returns("Hello Foo".Encode());
            dataProvider.Setup(d => d.GetObject("bar-oid", "blob")).Returns("Hello bar".Encode());
            fileOperator.Setup(d => d.Write("foo.txt", It.IsAny<byte[]>()));
            fileOperator.Setup(d => d.Write("bar.txt", It.IsAny<byte[]>()));
            Dictionary<string, string> index = new Dictionary<string, string>()
            {
                {"foo.txt", "foo-oid" },
                {"bar.txt", "bar-oid" }
            };
            treeOpeartion.CheckoutIndex(index);
            dataProvider.VerifyAll();
        }

        [TestMethod]
        public void ReadTreeTest()
        {
            dataProvider.Setup(d => d.Index).Returns(new Dictionary<string, string>()
            {
            });

            dataProvider.Setup(d => d.GetObject("foo-oid", "tree")).Returns(string.Join("\n", new[]
            {
                "blob a-oid a.txt",
                "tree bar-oid bar"
            }).Encode());

            dataProvider.Setup(d => d.GetObject("bar-oid", "tree")).Returns(string.Join("\n", new[]
            {
                "blob b-oid b.txt",
                "blob c-oid c.md"
            }).Encode());

            var tree = treeOpeartion.GetTree("foo-oid", "");

            fileOperator.Setup(d => d.EmptyCurrentDirectory(this.dataProvider.Object.IsIgnore));
            dataProvider.Setup(d => d.GetObject("a-oid", "blob")).Returns("Hello a".Encode());
            dataProvider.Setup(d => d.GetObject("b-oid", "blob")).Returns("Hello b".Encode());
            dataProvider.Setup(d => d.GetObject("c-oid", "blob")).Returns("Hello c".Encode());
            fileOperator.Setup(d => d.Write("a.txt", It.IsAny<byte[]>()));
            fileOperator.Setup(d => d.Write(Path.Join("bar", "b.txt"), It.IsAny<byte[]>()));
            fileOperator.Setup(d => d.Write(Path.Join("bar", "c.md"), It.IsAny<byte[]>()));

            treeOpeartion.ReadTree("foo-oid", true);
            dataProvider.VerifyAll();
        }

        [TestMethod]
        public void WriteTreeTest()
        {
            dataProvider.Setup(d => d.Index).Returns(new Dictionary<string, string>()
            {
                { Path.Join("foo.txt"), "foo-oid" },
                { Path.Join("sub", "bar.md"), "bar-oid" }
            });

            dataProvider.Setup(d => d.WriteObject(It.IsAny<byte[]>(), "tree")).Returns<byte[], string>((bytes, type)=>
            {
                if (bytes.Length == "blob bar-oid bar.md".Encode().Length)
                {
                    return "sub-oid";
                }
                if (bytes.Length == "blob foo-oid foo.txt\ntree sub-oid sub".Encode().Length)
                {
                    return "total-oid";
                }

                return string.Empty;
            });
            var actual = treeOpeartion.WriteTree();

            Assert.AreEqual("total-oid", actual);
            dataProvider.VerifyAll();
        }

        [TestMethod]
        public void GetWorkingTree()
        {
            fileOperator.Setup(d => d.Walk(".")).Returns(new[]
            {
                Path.Join(".", "foo.txt"),
                Path.Join(".", ".ugit", "index"),
                Path.Join(".", "sub", "bar.md"),
            });

            dataProvider.Setup(d => d.IsIgnore(It.IsAny<string>())).Returns<string>((path) =>
            {
                if(path == Path.Join(".", ".ugit", "index"))
                {
                    return true;
                }
                return false;
            });

            fileOperator.Setup(d => d.Read(It.IsAny<string>())).Returns<string>(path =>
            {
                if(path == Path.Join("foo.txt") || path == Path.Join("sub", "bar.md"))
                {
                    return Array.Empty<byte>();
                }
                throw new Exception();
            });

            this.dataProvider.Setup(d => d.WriteObject(It.IsAny<byte[]>(), "blob")).Returns("oid");

            var workingTree = treeOpeartion.GetWorkingTree();
            Assert.AreEqual(2, workingTree.Count);
            dataProvider.VerifyAll();
        }

    }
}
