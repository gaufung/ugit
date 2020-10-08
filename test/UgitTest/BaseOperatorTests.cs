using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.IO.Abstractions;

namespace Ugit
{
    [TestClass]
    public class BaseOperatorTests
    {
        private Mock<IFileSystem> fileSystemMock;

        private Mock<IDataProvider> dataProviderMock;

        private Mock<IDirectory> directoryMock;

        private IBaseOperator baseOperator;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>();
            dataProviderMock = new Mock<IDataProvider>();
            directoryMock = new Mock<IDirectory>();
            baseOperator = new BaseOperator(fileSystemMock.Object, dataProviderMock.Object);
        }

        [TestMethod]
        public void WriteTreeTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] { @".\hello.txt" });
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { @".\sub" });
            directoryMock.Setup(d => d.EnumerateFiles(@".\sub")).Returns(new[]
            {
                @".\sub\ugit.txt"
            });
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            baseOperator.WriteTree();
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }
    }
}
