using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Tindo.Ugit
{
    [TestClass]
    public class InitOperationTests
    {
        private Mock<IDataProvider> dataProviderMock;

        private InitOperation initOperation;

        [TestInitialize]
        public void Init()
        {
            dataProviderMock = new Mock<IDataProvider>();
            initOperation = new InitOperation(dataProviderMock.Object, NullLogger<InitOperation>.Instance);
        }

        [TestMethod]
        public void InitTest()
        {
            dataProviderMock.Setup(d => d.Init());
            dataProviderMock.Setup(d => d.UpdateRef("HEAD", It.IsAny<RefValue>(), true));
            initOperation.Init();
            dataProviderMock.VerifyAll();

        }


    }
}
