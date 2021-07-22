using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;

namespace Tindo.Ugit
{
    [TestClass]
    public class AddOpeartionTests
    {
        private Mock<IDataProvider> dataProvider;
        private Mock<IFileOperator> fileOperator;

        private IAddOperation addOperation;

        [TestInitialize]
        public void Init()
        {
            dataProvider = new Mock<IDataProvider>(MockBehavior.Strict);
            fileOperator = new Mock<IFileOperator>(MockBehavior.Strict);
            dataProvider.Setup(d => d.FileOperator).Returns(fileOperator.Object);
            addOperation = new AddOperation(dataProvider.Object);
        }

        [TestMethod]
        public void AddTest()
        {
            this.dataProvider.Setup(d => d.Index).Returns(new Tree());
            string[] fileNames = new string[]
            {
                Path.Join(".", "hello.txt"),
                Path.Join(".", "sub"),
                Path.Join(".", ".ugit", "HEAD"),
            };

            this.fileOperator.Setup(d => d.Exists(Path.Join(".", "hello.txt"), true)).Returns(true);
            this.fileOperator.Setup(d => d.Exists(Path.Join(".", ".ugit", "HEAD"), true)).Returns(true);
            this.fileOperator.Setup(d => d.Exists(Path.Join(".", "sub"), true)).Returns(false);
            this.fileOperator.Setup(d => d.Exists(Path.Join(".", "sub"), false)).Returns(true);

            this.dataProvider.Setup(d => d.IsIgnore(It.IsAny<string>())).Returns<string>(s => s.Contains(".ugit"));
            this.fileOperator.Setup(d => d.Read(It.IsAny<string>())).Returns(Array.Empty<byte>());
            this.fileOperator.Setup(d => d.Walk(Path.Join(".", "sub"))).Returns(new string[]{
               Path.Join(".", "sub", "foo.txt") 
            });
            this.dataProvider.Setup(d => d.WriteObject(It.IsAny<byte[]>(), "blob")).Returns("bar-oid");
            this.dataProvider.SetupSet(d => d.Index = It.IsAny<Tree>()).Verifiable();
            addOperation.Add(fileNames);
            this.dataProvider.VerifyAll();
        }
    }
}