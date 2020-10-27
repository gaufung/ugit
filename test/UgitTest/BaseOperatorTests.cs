using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Runtime.Serialization;
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

        private Mock<IDiff> diffMock;

        private IBaseOperator baseOperator;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>();
            dataProviderMock = new Mock<IDataProvider>();
            directoryMock = new Mock<IDirectory>();
            fileMock = new Mock<IFile>();
            diffMock = new Mock<IDiff>();
            baseOperator = new DefaultBaseOperator(fileSystemMock.Object, dataProviderMock.Object, diffMock.Object);
        }

        [TestMethod]
        public void WriteTreeTest()
        {
            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>()
            {
                { "hello.txt", "foo" },
                { Path.Join("sub", "world.txt"), "bar" },
                { Path.Join("sub", "ugit.txt"), "baz" },
            });
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));
            dataProviderMock.Setup(d => d.HashObject(It.IsAny<byte[]>(), "tree")).Returns("foobar");
            string actual = baseOperator.WriteTree();
            Assert.AreEqual("foobar", actual);
        }


        [TestMethod]
        public void ReadTreeTest()
        {
            string tree1 = string.Join("\n", new string[]
            {
                "blob oid1 hello.txt",
                "tree oid2 sub",
            });

            string tree2 = string.Join("\n", new string[]
            {
                "blob oid3 world.txt",
            });

            dataProviderMock.Setup(d => d.GetObject("oid4", "tree")).Returns(tree1.Encode());
            dataProviderMock.Setup(d => d.GetObject("oid2", "tree")).Returns(tree2.Encode());

            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>());
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));

            baseOperator.ReadTree("oid4", false);
            dataProviderMock.VerifyAll();
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
            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>());
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));
            dataProviderMock.Setup(f => f.GetRef("HEAD", true)).Returns(RefValue.Create(false, "baz"));
            dataProviderMock.Setup(f => f.UpdateRef("HEAD", RefValue.Create(false, "bar"), true));
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
            Assert.AreEqual("bar", commit.Parents[0]);
            Assert.AreEqual("Hello world\nThis is from ugit", commit.Message);
        }

        [TestMethod]
        [Ignore]
        public void CheckoutFalseRefValueTest()
        {
            string commitMessage = string.Join("\n", new string[]
            {
                "tree foo",
                "",
                "Hello world",
            });
            string oid = "Hello world".Encode().Sha1HexDigest();
            dataProviderMock.Setup(f => f.GetObject(oid, "commit")).Returns(commitMessage.Encode());
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(Array.Empty<string>());
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(Array.Empty<string>());
            string entry = "blob bar hello.txt";
            dataProviderMock.Setup(f => f.GetObject("foo", "tree")).Returns(entry.Encode());
            directoryMock.Setup(d => d.Exists("")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(Path.Join("", "hello.txt"), null));
            dataProviderMock.Setup(d => d.GetObject("bar", "blob")).Returns((byte[])null);
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", RefValue.Create(false, oid), false));
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>());
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));
            baseOperator.Checkout(oid);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        [Ignore]
        public void CheckoutTrueRefValueTest()
        {
            string commitMessage = string.Join("\n", new string[]
            {
                "tree foo",
                "",
                "Hello world",
            });
            string name = "master";
            string oid = "Hello world".Encode().Sha1HexDigest();
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs","heads",name), false)).Returns(RefValue.Create(false, oid));
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "heads", name), true)).Returns(RefValue.Create(false, oid));
            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>());
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));
            dataProviderMock.Setup(f => f.GetObject(oid, "commit")).Returns(commitMessage.Encode());
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(Array.Empty<string>());
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(Array.Empty<string>());
            string entry = "blob bar hello.txt";
            dataProviderMock.Setup(f => f.GetObject("foo", "tree")).Returns(entry.Encode());
            directoryMock.Setup(d => d.Exists(".")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(Path.Join(".", "hello.txt"), null));
            dataProviderMock.Setup(d => d.GetObject("bar", "blob")).Returns((byte[])null);
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", RefValue.Create(true, Path.Join("refs", "heads", name)), false));
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            baseOperator.Checkout(name);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void GetOidTest()
        {
            string name = "foo";
            dataProviderMock.Setup(d => d.GetRef(name, false)).Returns(RefValue.Create(false, "bar"));
            dataProviderMock.Setup(d => d.GetRef(name, true)).Returns(RefValue.Create(false, "bar"));
            Assert.AreEqual("bar", baseOperator.GetOid(name));
        }

        [TestMethod]
        public void GetOidIllegalTest()
        {
            string name = "foo";
            dataProviderMock.Setup(d => d.GetRef(name, true)).Returns(RefValue.Create(false, null));
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "tags", name), true)).Returns(RefValue.Create(false, null));
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "heads", name), true)).Returns(RefValue.Create(false, null));
            Assert.IsNull(baseOperator.GetOid(name));
        }

        [TestMethod]
        public void TestOidCommitId()
        {
            string commitId = "Hello World".Encode().Sha1HexDigest();
            dataProviderMock.Setup(d => d.GetRef(commitId, true)).Returns(RefValue.Create(false, null));
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "tags", commitId), true)).Returns(RefValue.Create(false, null));
            dataProviderMock.Setup(d => d.GetRef(Path.Join("refs", "heads", commitId), true)).Returns((RefValue.Create(false, null)));
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

        [TestMethod]
        public void CreateBranchTest()
        {
            string name = "master";
            string @ref = Path.Join("refs", "heads", "master");
            dataProviderMock.Setup(d => d.UpdateRef(@ref, RefValue.Create(false, "foo"), true));
            baseOperator.CreateBranch(name, "foo");
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void InitTest()
        {
            dataProviderMock.Setup(d => d.Init());
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", RefValue.Create(true, Path.Join("refs", "heads", "master")), true));
            baseOperator.Init();
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void IterBranchNamesTest()
        {
            dataProviderMock.Setup(d => d.IterRefs(Path.Join("refs", "heads"), true)).Returns(new (string, RefValue)[]
            {
                (Path.Join("refs", "heads", "master"), RefValue.Create(false, "foo")),
                (Path.Join("refs", "heads", "dev"), RefValue.Create(false, "bar")),
                (Path.Join("refs", "heads", "test"), RefValue.Create(false, "baz"))
            });
            string[] expected = new string[]
            {
                "master",
                "dev",
                "test"
            };
            CollectionAssert.AreEqual(expected, baseOperator.IterBranchNames().ToArray());
        }

        [TestMethod]
        public void ResetTest()
        {
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", RefValue.Create(false, "foo"), true));
            baseOperator.Reset("foo");
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void GetWorkingTreeTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new string[]
            {
                Path.Join(".", "foo.txt")
            });

            fileMock.Setup(f => f.Exists("foo.txt")).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes("foo.txt")).Returns("Hello World".Encode());
            dataProviderMock.Setup(d => d.HashObject(It.IsAny<Byte[]>(), It.IsAny<string>())).Returns("bar");
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var actual = baseOperator.GetWorkingTree();
            Assert.IsTrue(actual.ContainsKey("foo.txt"));
            Assert.AreEqual("bar", actual["foo.txt"]);
        }

        [TestMethod]
        public void MergeTest()
        {
            dataProviderMock.Setup(d => d.GetRef("HEAD", true)).Returns(RefValue.Create(false, "foo"));
            dataProviderMock.Setup(d => d.GetObject("foo", "commit")).Returns(string.Join("\n", new string[]
            {
                "tree foo1",
                "parent foo2",
                "",
                "this is for foo"
            }).Encode());

            string other = "bar";
            dataProviderMock.Setup(d => d.GetObject("bar", "commit")).Returns(string.Join("\n", new string[]
            {
                "tree bar1",
                "parent bar2",
                "",
                "this is for bar"
            }).Encode());

            dataProviderMock.Setup(d => d.GetIndex()).Returns(new Dictionary<string, string>());
            dataProviderMock.Setup(d => d.SetIndex(It.IsAny<Dictionary<string, string>>()));
            dataProviderMock.Setup(d => d.UpdateRef("MERGE_HEAD", RefValue.Create(false, "bar"), true));
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(Array.Empty<string>());
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(Array.Empty<string>());
            diffMock.Setup(d => d.MergeTree(It.IsAny<Dictionary<string, string>>(), It.IsAny<Dictionary<string, string>>())).Returns(new Dictionary<string, string>() { { "foo.txt", "oid1"} });
            fileMock.Setup(f => f.WriteAllBytes("foo.txt", It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            baseOperator.Merge(other);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
            diffMock.VerifyAll();
        }

        [TestMethod]
        public void GetMergeBaseTest()
        {
            dataProviderMock.Setup(d => d.GetObject("foo", "commit")).Returns(string.Join("\n", new[]
            {
                "tree foo",
                "parent foobar",
                "",
                "this is for foo"
            }).Encode());

            dataProviderMock.Setup(d => d.GetObject("bar", "commit")).Returns(string.Join("\n", new[]
            {
                "tree bar",
                "parent foobar",
                "",
                "this is for bar"
            }).Encode());

            string acutal = baseOperator.GetMergeBase("foo", "bar");
            Assert.AreEqual("foobar", acutal);
            dataProviderMock.VerifyAll();
        }
    }
}
