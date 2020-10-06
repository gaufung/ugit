using System.IO.Abstractions;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ugit
{
    [TestClass]
    public class BaseTests
    {
        private Mock<IFileSystem> fileSystemMock;

        private Mock<ICommandProcess> commandProcessMock;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>();
            commandProcessMock = new Mock<ICommandProcess>();
        }

        [TestMethod]
        public void TestWriteTree()
        {
             
        }
        
        
    }
}