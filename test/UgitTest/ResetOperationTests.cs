﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tindo.Ugit;

namespace Tindo.Ugit
{
    [TestClass]
    public class ResetOperationTests
    {
        private Mock<IDataProvider> dataProvider;

        private IResetOperation resetOpreation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            resetOpreation = new ResetOperation(dataProvider.Object);
        }

        [TestMethod]
        public void ResetTest()
        {
            dataProvider.Setup(d => d.UpdateRef("HEAD", It.Is<RefValue>(r => !r.Symbolic && r.Value == "foo-oid"), true));
            resetOpreation.Reset("foo-oid");
            dataProvider.VerifyAll();
        }
    }
}
