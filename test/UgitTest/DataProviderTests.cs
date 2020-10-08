using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.IO.Abstractions;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class DataProviderTests
    {
        private Mock<IDirectory> direcotryMock;

        private Mock<IPath> pathMock;

        private Mock<IFileSystem> fileSystemMock;

        private Mock<IFile> fileMock;

        private IDataProvider provider;

        [TestInitialize]
        public void Init()
        {
            direcotryMock = new Mock<IDirectory>(MockBehavior.Loose);
            pathMock = new Mock<IPath>(MockBehavior.Loose);
            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Loose);
            fileMock = new Mock<IFile>(MockBehavior.Loose);
            provider = new DataProvider(fileSystemMock.Object);
        }

        [TestMethod]
        public void InitTest()
        {
            direcotryMock.Setup(d => d.CreateDirectory(".ugit"));
            string directoryPath = Path.Combine(".ugit", "objects");
            direcotryMock.Setup(d => d.CreateDirectory(directoryPath));
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            provider.Init();
            direcotryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GitDirTest()
        {
            Assert.AreEqual(".ugit", provider.GitDir);
            direcotryMock.Setup(d => d.GetCurrentDirectory()).Returns(@"D:\test");
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            string expected = Path.Join(@"D:\test", ".ugit");
            Assert.AreEqual(expected, provider.GitDirFullPath);
            direcotryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void HashObjectTest()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello World");
            string filePath = Path.Join(".ugit", "objects", "0a4d55a8d778e5022fab701977c5d840bbc486d0");
            fileMock.Setup(f => f.WriteAllBytes(filePath, data));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var actual = provider.HashObject(data);
            Assert.AreEqual("0a4d55a8d778e5022fab701977c5d840bbc486d0", actual);
        }
    }
}
