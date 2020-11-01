using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class DataProviderTests
    {
        private Mock<IDirectory> direcotryMock;

        private Mock<IFileSystem> fileSystemMock;

        private Mock<IFile> fileMock;

        private IDataProvider dataProvider;

        [TestInitialize]
        public void Init()
        {
            direcotryMock = new Mock<IDirectory>(MockBehavior.Loose);
            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Loose);
            fileMock = new Mock<IFile>(MockBehavior.Loose);
            dataProvider = new DefaultDataProvider(fileSystemMock.Object);
        }

        [TestMethod]
        public void InitTest()
        {
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(false);
            direcotryMock.Setup(d => d.CreateDirectory(".ugit"));
            string directoryPath = Path.Combine(".ugit", "objects");
            direcotryMock.Setup(d => d.CreateDirectory(directoryPath));
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.Init();
            direcotryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void InitTwiceTest()
        {
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            direcotryMock.Setup(d => d.Delete(".ugit", true));
            direcotryMock.Setup(d => d.CreateDirectory(".ugit"));
            string directoryPath = Path.Combine(".ugit", "objects");
            direcotryMock.Setup(d => d.CreateDirectory(directoryPath));
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.Init();
            direcotryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GitDirTest()
        {
            Assert.AreEqual(".ugit", dataProvider.GitDir);
            direcotryMock.Setup(d => d.GetCurrentDirectory()).Returns(@"D:\test");
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            string expected = Path.Join(@"D:\test", ".ugit");
            Assert.AreEqual(expected, dataProvider.GitDirFullPath);
            direcotryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void HashObjectTest()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello World");
            string filePath = Path.Join(".ugit", "objects", "0a6649a0077da1bf5a8b3b5dd3ea733ea6a81938");
            fileMock.Setup(f => f.WriteAllBytes(filePath, data));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var actual = dataProvider.HashObject(data);
            Assert.AreEqual("0a6649a0077da1bf5a8b3b5dd3ea733ea6a81938", actual);
        }

        [TestMethod]
        public void GetObjectNonExistTest()
        {
            string oid = "foo";
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(false);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var expected = Array.Empty<byte>();
            CollectionAssert.AreEqual(expected, dataProvider.GetObject(oid));
        }

        [TestMethod]
        public void GetObjectExistTest()
        {
            string oid = "foo";
            byte[] data = "Hello World".Encode();
            data = "blob".Encode().Concat(new[] { byte.Parse("0") }).Concat(data).ToArray();
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(data);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var expected = "Hello World".Encode();
            CollectionAssert.AreEqual(expected, dataProvider.GetObject(oid));
        }

        [TestMethod]
        public void UpdateRefTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            string oid = "foo";
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(filePath, It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void UpdateRefTrueTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            string @ref = Path.Join(".ugit", "heads", "master");
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(filePath, It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.UpdateRef("HEAD", RefValue.Create(true, @ref));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GetRefNullTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(false);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.IsNull(dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GetRefTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            string head = "Hello World";
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(head.Encode());
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.AreEqual(head, dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GeRefDerefTest()
        {
            string headFilePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(headFilePath)).Returns(true);
            string head = "ref:master";
            fileMock.Setup(f => f.ReadAllBytes(headFilePath)).Returns(head.Encode());
            string masterFilePath = Path.Join(".ugit", "master");
            fileMock.Setup(f => f.Exists(masterFilePath)).Returns(true);
            string master = "foo";
            fileMock.Setup(f => f.ReadAllBytes(masterFilePath)).Returns(master.Encode());
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.AreEqual("foo", dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();


        }

        [TestMethod]
        public void IterRefsTest()
        {
            string headPath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(headPath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(headPath)).Returns("foo".Encode());

            string tagPath = Path.Join(".ugit", "refs", "tags", "v1.0");
            fileMock.Setup(f => f.Exists(tagPath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(tagPath)).Returns("bar".Encode());

            direcotryMock.Setup(d => d.EnumerateFiles(Path.Join(".ugit", "refs"))).Returns(Array.Empty<string>());
            direcotryMock.Setup(d => d.EnumerateDirectories(Path.Join(".ugit", "refs"))).Returns(new string[]
            {
                Path.Join(".ugit", "refs", "tags")
            });

            direcotryMock.Setup(d => d.EnumerateFiles(Path.Join(".ugit", "refs", "tags"))).Returns(new string[]
            {
                Path.Join(".ugit", "refs", "tags", "v1.0")
            });
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            (string, RefValue)[] refs = dataProvider.IterRefs().ToArray();
            CollectionAssert.AreEqual(new (string, RefValue)[]
            {
                ValueTuple.Create("HEAD", RefValue.Create(false, "foo")),
                ValueTuple.Create(Path.Join("refs", "tags", "v1.0"), RefValue.Create(false, "bar"))
            }, refs);
        }

        [TestMethod]
        public void DeleteRefTest()
        {
            string @ref = Path.Join("refs", "head", "dev");
            fileMock.Setup(f => f.Exists(Path.Join(".ugit", @ref))).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(Path.Join(".ugit", @ref))).Returns("foo".Encode());
            fileMock.Setup(f => f.Delete(Path.Join(".ugit", @ref)));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProvider.DeleteRef(@ref);
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }
    }
}
