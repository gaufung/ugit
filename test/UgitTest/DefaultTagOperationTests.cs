using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using Ugit.Operations;

namespace Ugit
{
    [TestClass]
    public class DefaultTagOperationTests
    {
        private Mock<IDataProvider> dataProviderMock;

        private ITagOperation tagOperation;

        [TestInitialize]
        public void Init()
        {
            dataProviderMock = new Mock<IDataProvider>();
            tagOperation = new DefaultTagOperation(dataProviderMock.Object);
        }

        [TestMethod]
        public void CreateTest()
        {
            dataProviderMock.Setup(d => d.UpdateRef(Path.Join("refs", "tags", "foo-tag"), It.IsAny<RefValue>(), true));
            tagOperation.Create("foo-tag", "");
            dataProviderMock.VerifyAll();
        }
    }
}
