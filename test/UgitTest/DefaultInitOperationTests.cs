using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ugit.Operations;

namespace Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultInitOperationTests
    {
        private Mock<IDataProvider> dataProviderMock;

        private DefaultInitOperation initOperation;

        [TestInitialize]
        public void Init()
        {
            dataProviderMock = new Mock<IDataProvider>();
            initOperation = new DefaultInitOperation(dataProviderMock.Object);
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
