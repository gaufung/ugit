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
            dataProvider = new DataProvider(fileSystemMock.Object);
        }

        [TestMethod]
        public void InitTest()
        {
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
        public void SetHEADTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            string oid = "foo";
            fileMock.Setup(f => f.WriteAllBytes(filePath, It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProvider.SetRef("HEAD", oid);
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GetHEADNullTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(false);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.IsNull(dataProvider.GetRef("HEAD"));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GetHEADTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            string head = "Hello World";
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(head.Encode());
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.AreEqual(head, dataProvider.GetRef("HEAD"));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }
    }
}
