using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Linq;
using Tindo.Ugit;

namespace Tindo.Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultDiffTests
    {

        private IDiffOperation diff;

        private Mock<IDataProvider> dataproviderMock;

        private Mock<IDiffProxy> diffProxyMock;

        private Mock<IFileOperator> fileOperator;

        [TestInitialize]
        public void Init()
        {
            dataproviderMock = new Mock<IDataProvider>();
            diffProxyMock = new Mock<IDiffProxy>();
            fileOperator = new Mock<IFileOperator>();
            diff = new DiffOperation(dataproviderMock.Object, diffProxyMock.Object);
        }

        [TestMethod]
        public void CompareTreesTest()
        {
            Tree fromTree = new Tree
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Tree toTree = new Tree
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
            };

            (string, IEnumerable<string>)[] expected = new (string, IEnumerable<string>)[]
            {
                ("hello.txt", new string[2]{"foo", "foo1"}),
                ("ugit.txt", new string[2]{"baz", null}),
                ("world.txt", new string[2]{"bar", "bar"}),
            };
            (string, IEnumerable<string>)[] actual = diff.CompareTrees(fromTree, toTree).OrderBy(i=>i.Item1).ToArray();
            for(int i = 0; i < 2; i++)
            {
                Assert.AreEqual(expected[i].Item1, actual[i].Item1);
                CollectionAssert.AreEqual(expected[i].Item2.ToArray(), actual[i].Item2.ToArray());
            }
        }

        [TestMethod]
        public void DiffTreeTest()
        {
            Tree fromTree = new Tree
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Tree toTree = new Tree
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
            };
            fileOperator.Setup(f => f.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            diffProxyMock.Setup(d => d.Execute("diff", It.IsAny<string>())).Returns<string, string>( (_, args) => 
            {
                if (args.Contains("hello.txt"))
                {
                    return (0, "foo", "");
                }

                if(args.Contains("ugit.txt"))
                {
                    return (0, "bar", "");
                }

                return (-1, "", "");

            });
            string actual = diff.DiffTrees(fromTree, toTree);
            Assert.AreEqual("foo\nbar", actual);
        }

        [TestMethod]
        public void IterChangedFilesTest()
        {
            Tree fromTree = new Tree
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Tree toTree = new Tree
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
                { "helloWorld.txt", "foobar" },
            };

            (string, string)[] actual = diff.IterChangedFiles(fromTree, toTree).ToArray();
            CollectionAssert.AreEqual(new (string, string)[]
            {
                ("hello.txt", "modified"),
                ("ugit.txt", "deleted"),
                ("helloWorld.txt", "new file"),
            }, actual);
        }

        [TestMethod]
        public void MergeTreeTest()
        {
            Tree headTree = new Tree
            {
                { "hello.txt", "foo" },
            };
            Tree otherTree = new Tree
            {
                { "hello.txt", "foo1" },
            };

            dataproviderMock.Setup(d => d.GetObject("foo", "blob")).Returns("hello".Encode());
            dataproviderMock.Setup(d => d.GetObject("foo1", "blob")).Returns("Hello".Encode());
            dataproviderMock.Setup(d => d.WriteObject(It.IsAny<byte[]>(), "blob")).Returns("foo");
            fileOperator.Setup(f => f.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            diffProxyMock.Setup(d => d.Execute(It.IsAny<string>(), It.IsAny<string>())).Returns((0, "Hello", ""));
            var acutal = diff.MergeTree(headTree, otherTree);
            Assert.AreEqual("foo", acutal["hello.txt"]);
        }
    }
}
