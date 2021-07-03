using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Ugit.Operations;
using System.IO;
using System;
using System.Collections.Generic;

namespace Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultAddOpeartionTests
    {
        private Mock<IDataProvider> dataProvider;
        private Mock<IFileOperator> fileOperator;

        private IAddOperation addOperation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>();
            fileOperator = new Mock<IFileOperator>();
            addOperation = new DefaultAddOperation(dataProvider.Object, fileOperator.Object);
        }

        [TestMethod]
        public void AddTest()
        {
            this.dataProvider.Setup(d => d.Index).Returns(new Dictionary<string, string>());
            string[] fileNames = new string[]
            {
                Path.Join(".", "hello.txt"),
                Path.Join(".", "sub"),
                Path.Join(".", ".ugit", "HEAD"),
            };

            this.fileOperator.Setup(d => d.Exist(Path.Join(".", "hello.txt"), true)).Returns(true);
            this.fileOperator.Setup(d => d.Exist(Path.Join(".", ".ugit", "HEAD"), true)).Returns(true);
            this.fileOperator.Setup(d => d.Exist(Path.Join(".", "sub"), true)).Returns(false);
            this.fileOperator.Setup(d => d.Exist(Path.Join(".", "sub"), false)).Returns(true);

            this.dataProvider.Setup(d => d.IsIgnore(It.IsAny<string>())).Returns<string>(s => s.Contains(".ugit"));
            this.fileOperator.Setup(d => d.Read(It.IsAny<string>())).Returns(Array.Empty<byte>());
            this.fileOperator.Setup(d => d.Walk(Path.Join(".", "sub"))).Returns(new string[]{
               Path.Join(".", "sub", "foo.txt") 
            });
            this.dataProvider.Setup(d => d.HashObject(It.IsAny<byte[]>(), "blob")).Returns("bar-oid");
            addOperation.Add(fileNames);
            this.dataProvider.VerifyAll();
        }
    }
}