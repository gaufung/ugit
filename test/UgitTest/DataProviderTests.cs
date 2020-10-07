using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.IO.Abstractions;

namespace Ugit
{
    [TestClass]
    public class DataProviderTests
    {
        private Mock<IDirectory> direcotryMock;

        private Mock<IPath> pathMock;

        private Mock<IFileSystem> fileSystemMock;

        private IDataProvider provider;

        [TestInitialize]
        public void Init()
        {
            direcotryMock = new Mock<IDirectory>(MockBehavior.Strict);
            pathMock = new Mock<IPath>(MockBehavior.Strict);
            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);

            provider = new DataProvider(fileSystemMock.Object);
        }

        [TestMethod]
        public void TestGitDir()
        {
            Assert.AreEqual(".ugit", provider.GitDir);

            direcotryMock.Setup(d => d.GetCurrentDirectory()).Returns(@"D:\test");
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            string expected = Path.Join(@"D:\test", ".ugit");
            Assert.AreEqual(expected, provider.GitDirFullPath);
        }
    }
}
