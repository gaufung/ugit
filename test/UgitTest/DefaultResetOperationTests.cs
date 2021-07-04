using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tindo.Ugit.Operations;

namespace Tindo.Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultResetOperationTests
    {
        private Mock<IDataProvider> dataProvider;

        private IResetOperation resetOpreation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            resetOpreation = new DefaultResetOperation(dataProvider.Object);
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
