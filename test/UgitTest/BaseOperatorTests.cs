using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class BaseOperatorTests
    {
        private Mock<IFileSystem> fileSystemMock;

        private Mock<IDataProvider> dataProviderMock;

        private Mock<IDirectory> directoryMock;

        private Mock<IFile> fileMock;

        private IBaseOperator baseOperator;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>();
            dataProviderMock = new Mock<IDataProvider>();
            directoryMock = new Mock<IDirectory>();
            fileMock = new Mock<IFile>();
            baseOperator = new BaseOperator(fileSystemMock.Object, dataProviderMock.Object);
        }

        [TestMethod]
        public void WriteTreeTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] { $".{Path.DirectorySeparatorChar}hello.txt" });
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { $".{Path.DirectorySeparatorChar}sub", $".{Path.DirectorySeparatorChar}.ugit" });
            directoryMock.Setup(d => d.EnumerateFiles($".{Path.DirectorySeparatorChar}sub")).Returns(new[]
            {
                $".{Path.DirectorySeparatorChar}sub{Path.DirectorySeparatorChar}ugit.txt"
            });
            byte[] helloData = Encoding.UTF8.GetBytes("Hello World");
            byte[] ugitData = Encoding.UTF8.GetBytes("Hello Ugit");
            fileMock.Setup(f => f.ReadAllBytes($".{Path.DirectorySeparatorChar}hello.txt")).Returns(helloData);
            fileMock.Setup(f => f.ReadAllBytes($".{Path.DirectorySeparatorChar}sub{Path.DirectorySeparatorChar}ugit.txt")).Returns(ugitData);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProviderMock.Setup(f => f.GitDir).Returns(".ugit");
            string subStree = "blob bar ugit.txt";

            string tree = string.Join("\n", new string[]
            {
                $"blob foo hello.txt",
                $"tree baz sub"
            });

            dataProviderMock.Setup(f => f.HashObject(It.IsAny<byte[]>(), It.IsAny<string>())).Returns<byte[], string>((data, type)=>
            {
                if (data.SequenceEqual(Encoding.UTF8.GetBytes(subStree)) && type =="tree")
                {
                    return "baz";
                }

                if (data.SequenceEqual(Encoding.UTF8.GetBytes(tree)) && type == "tree")
                {
                    return "foobar";
                }

                if (data.SequenceEqual(helloData) && type =="blob")
                {
                    return "foo";
                }

                if(data.SequenceEqual(ugitData) && type == "blob")
                {
                    return "bar";
                }
                return null;
            });
            string expected = "foobar";
            Assert.AreEqual(expected, baseOperator.WriteTree());
            fileMock.VerifyAll();
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
            dataProviderMock.VerifyAll();
        }


        [TestMethod]
        public void ReadTreeTest()
        {
            string helloFilePath = $".{Path.DirectorySeparatorChar}hello.txt";
            string subDirectory = $".{Path.DirectorySeparatorChar}sub";
            string ugitDirectory = $".{Path.DirectorySeparatorChar}.ugit";
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] {helloFilePath});
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { helloFilePath, subDirectory });
            string treeOid = "foo";
            string tree = string.Join("\n", new string[]
            {
                "blob bar hello.txt",
                "tree baz sub"
            });

            string subTree = string.Join("\n", new string[]
            {
                "blob zoo ugit.txt"
            });
            dataProviderMock.Setup(d => d.GetObject(treeOid, "tree")).Returns(tree.Encode());
            dataProviderMock.Setup(d => d.GetObject("baz", "tree")).Returns(subTree.Encode());

            byte[] helloData = "Hello World".Encode();
            byte[] ugitData = "Hello Ugit".Encode();
            dataProviderMock.Setup(d => d.GetObject("bar", "blob")).Returns(helloData);
            dataProviderMock.Setup(d => d.GetObject("zoo", "blob")).Returns(ugitData);
            directoryMock.Setup(d => d.Exists(Path.Join(".", "sub"))).Returns(false);
            directoryMock.Setup(d => d.CreateDirectory(Path.Join(".", "sub")));
            fileMock.Setup(s => s.WriteAllBytes(Path.Join(".","hello.txt"), It.IsAny<byte[]>()));
            fileMock.Setup(s => s.WriteAllBytes(Path.Join(".", "sub", "ugit.txt"), It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            baseOperator.ReadTree(treeOid);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void CommitTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(Array.Empty<string>());
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(Array.Empty<string>());
            string message = "hello world";
            string commit = $"tree foo\nparent baz\n\n{message}\n";

            dataProviderMock.Setup(f => f.HashObject(It.IsAny<byte[]>(), It.IsAny<string>())).Returns<byte[], string>((data, type) =>
            {
                if(data.SequenceEqual(commit.Encode()) && type == "commit")
                {
                    return "bar";
                }
                return "foo";
            });
            dataProviderMock.Setup(f => f.GetRef("HEAD")).Returns("baz");
            dataProviderMock.Setup(f => f.UpdateRef("HEAD", "bar"));
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);

            string expected = "bar";
            Assert.AreEqual(expected, baseOperator.Commit(message));
        }


        [TestMethod]
        public void GetCommitTest()
        {
            string commitMessage = string.Join("\n", new string[]
            {
                "tree foo",
                "parent bar",
                "",
                "Hello world",
                "This is from ugit",
            });
            string oid = "this-oid";
            dataProviderMock.Setup(f => f.GetObject(oid, "commit")).Returns(commitMessage.Encode());
            var commit = baseOperator.GetCommit(oid);
            Assert.AreEqual("foo", commit.Tree);
            Assert.AreEqual("bar", commit.Parent);
            Assert.AreEqual("Hello world\nThis is from ugit", commit.Message);
        }

        [TestMethod]
        public void CheckoutTest()
        {
            string commitMessage = string.Join("\n", new string[]
            {
                "tree foo",
                "",
                "Hello world",
            });
            string oid = "this-oid";
            dataProviderMock.Setup(f => f.GetObject(oid, "commit")).Returns(commitMessage.Encode());
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(Array.Empty<string>());
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(Array.Empty<string>());
            string entry = "blob bar hello.txt";
            dataProviderMock.Setup(f => f.GetObject("foo", "tree")).Returns(entry.Encode());
            directoryMock.Setup(d => d.Exists(".")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(Path.Join(".", "hello.txt"), null));
            dataProviderMock.Setup(d => d.GetObject("bar", "blob")).Returns((byte[])null);
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", oid));
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            baseOperator.Checkout(oid);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void GetOidTest()
        {
            string name = "foo";
            dataProviderMock.Setup(d => d.GetRef(name)).Returns("bar");
            Assert.AreEqual("bar", baseOperator.GetOid(name));
        }

        [TestMethod]
        public void GetOidIllegalTest()
        {
            string name = "foo";
            dataProviderMock.Setup(d => d.GetRef(name)).Returns((string)null);
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "tags", name))).Returns((string)null);
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "heads", name))).Returns((string)null);
            Assert.IsNull(baseOperator.GetOid(name));
        }

        [TestMethod]
        public void TestOidCommitId()
        {
            string commitId = "Hello World".Encode().Sha1HexDigest();
            dataProviderMock.Setup(d => d.GetRef(commitId)).Returns((string)null);
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "tags", commitId))).Returns((string)null);
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "heads", commitId))).Returns((string)null);
            Assert.AreEqual(commitId, baseOperator.GetOid(commitId));
        }

        [TestMethod]
        public void IterCommitsAndParentsTest()
        {
            string oid = "foo";
            string messageFoo = string.Join("\n", new string[]
            {
                "tree foo",
                "parent baz",
                "\n",
                "this is second commit"
            });

            string messageBaz = string.Join("\n", new string[]
            {
                "tree baz",
                "\n",
                "this is first commit"
            });

            dataProviderMock.Setup(d => d.GetObject("foo", "commit")).Returns(messageFoo.Encode());
            dataProviderMock.Setup(d => d.GetObject("baz", "commit")).Returns(messageBaz.Encode());
            var commits = baseOperator.IterCommitsAndParents(new string[] { oid }).ToArray();
            CollectionAssert.AreEqual(new string[] { "foo", "baz" }, commits);
        }
    }
}
