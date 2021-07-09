using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using Tindo.Ugit;

namespace Tindo.Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultTagOperationTests
    {
        private Mock<IDataProvider> dataProviderMock;

        private ITagOperation tagOperation;

        [TestInitialize]
        public void Init()
        {
            dataProviderMock = new Mock<IDataProvider>();
            tagOperation = new TagOperation(dataProviderMock.Object);
        }

        [TestMethod]
        public void CreateTest()
        {
            dataProviderMock.Setup(d => d.UpdateRef(Path.Join("refs", "tags", "foo-tag"), It.IsAny<RefValue>(), true));
            tagOperation.Create("foo-tag", "");
            dataProviderMock.VerifyAll();
        }

        [TestMethod]
        public void AllTest()
        {
            string prefix = Path.Join("refs", "tags");
            dataProviderMock.Setup(d => d.GetAllRefs(prefix, false)).Returns(new[] { 
                ValueTuple.Create(Path.Join(prefix, "v1.0"), RefValue.Create(false, "oid1")),
                ValueTuple.Create(Path.Join(prefix, "v1.1"), RefValue.Create(false, "oid1.1")),
            });

            var tags = tagOperation.All.ToArray();
            CollectionAssert.AreEqual(new string[] { "v1.0", "v1.1" }, tags);

        }
    }
}
