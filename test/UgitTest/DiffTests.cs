using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Ugit
{
    [TestClass]
    public class DiffTests
    {

        private IDiff diff;

        private Mock<IDataProvider> dataproviderMock;

        private Mock<IDiffProxy> diffProxyMock;

        private Mock<IFileSystem> fileSystemMock;

        private Mock<IFile> fileMock;

        [TestInitialize]
        public void Init()
        {
            dataproviderMock = new Mock<IDataProvider>();
            diffProxyMock = new Mock<IDiffProxy>();
            fileSystemMock = new Mock<IFileSystem>();
            fileMock = new Mock<IFile>();
            diff = new DefaultDiff(dataproviderMock.Object, diffProxyMock.Object, fileSystemMock.Object);
        }

        [TestMethod]
        public void CompareTreesTest()
        {
            Dictionary<string, string> fromTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Dictionary<string, string> toTree = new Dictionary<string, string>
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
            Dictionary<string, string> fromTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Dictionary<string, string> toTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo1" },
                { "world.txt", "bar" },
            };
            fileMock.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
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
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            string actual = diff.DiffTrees(fromTree, toTree);
            Assert.AreEqual("foo\nbar", actual);
        }

        [TestMethod]
        public void IterChangedFilesTest()
        {
            Dictionary<string, string> fromTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
                { "world.txt", "bar" },
                { "ugit.txt", "baz" },
            };
            Dictionary<string, string> toTree = new Dictionary<string, string>
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
            Dictionary<string, string> headTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo" },
            };
            Dictionary<string, string> otherTree = new Dictionary<string, string>
            {
                { "hello.txt", "foo1" },
            };

            dataproviderMock.Setup(d => d.GetObject("foo", "blob")).Returns("hello".Encode());
            dataproviderMock.Setup(d => d.GetObject("foo1", "blob")).Returns("Hello".Encode());
            dataproviderMock.Setup(d => d.HashObject(It.IsAny<byte[]>(), "blob")).Returns("foo");
            fileMock.Setup(f => f.WriteAllBytes(It.IsAny<string>(), It.IsAny<byte[]>()));
            diffProxyMock.Setup(d => d.Execute(It.IsAny<string>(), It.IsAny<string>())).Returns((0, "Hello", ""));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var acutal = diff.MergeTree(headTree, otherTree);
            Assert.AreEqual("foo", acutal["hello.txt"]);
        }
    }
}
