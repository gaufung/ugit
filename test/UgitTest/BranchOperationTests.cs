﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Linq;

namespace Tindo.Ugit
{
    [TestClass]
    public class BranchOperationTests
    {
        private Mock<IDataProvider> dataProvider;

        private IBranchOperation branchOperation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            branchOperation = new BranchOperation(dataProvider.Object);
        }

        [TestMethod]
        public void NamesTest()
        {
            dataProvider.Setup(d => d.GetAllRefs(Path.Join("refs", "heads"), true)).Returns(new [] { 
                (Path.Join("refs", "heads", "master"), RefValue.Create(false, "foo")),
                (Path.Join("refs", "heads", "dev"), RefValue.Create(false, "bar")),
                (Path.Join("refs", "heads", "test"), RefValue.Create(false, "baz")),
            });

            string[] actual = branchOperation.Names.ToArray();
            CollectionAssert.AreEqual(new[] { "master", "dev", "test" }, actual);
        }

        [TestMethod]
        public void CurrentNullTest()
        {
            dataProvider.Setup(d => d.GetRef("HEAD", false)).Returns(RefValue.Create(false, ""));
            Assert.IsNull(branchOperation.Current);
        }

        [TestMethod]
        public void CurrentBranchTest()
        {
            dataProvider.Setup(d => d.GetRef("HEAD", false)).Returns(RefValue.Create(true, Path.Join("refs", "heads", "master")));
            Assert.AreEqual("master", branchOperation.Current);
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void CurrentIllegalTest()
        {
            dataProvider.Setup(d => d.GetRef("HEAD", false)).Returns(RefValue.Create(true, Path.Join("refs", "master")));
            var _ = branchOperation.Current;
        }

        [TestMethod]
        public void CreateTest()
        {
            string @ref = Path.Join("refs", "heads", "dev");
            dataProvider.Setup(d => d.UpdateRef(@ref, It.IsAny<RefValue>(), true));

            branchOperation.Create("dev", "foo");
            dataProvider.VerifyAll();
        }


        [TestMethod]
        public void IsBranchTest()
        {
            string path = Path.Join("refs", "heads", "master");
            dataProvider.Setup(d => d.GetRef(path, true)).Returns(RefValue.Create(false, "foo"));
            Assert.IsTrue(branchOperation.IsBranch("master"));
        }

        [TestMethod]
        public void IsBranchFalseTest()
        {
            string path = Path.Join("refs", "heads", "dev");
            dataProvider.Setup(d => d.GetRef(path, true)).Returns(RefValue.Create(false, ""));
            Assert.IsFalse(branchOperation.IsBranch("dev"));
        }
    }
}
