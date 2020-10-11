using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Ugit
{
    [TestClass]
    public class BaseOperatorTests
    {
        private Mock<IFileSystem> fileSystemMock;

        private Mock<IDataProvider> dataProviderMock;

        private Mock<IDirectory> directoryMock;

        private Mock<IFile> fileMock;

        private IBaseOperator baseOperator;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>();
            dataProviderMock = new Mock<IDataProvider>();
            directoryMock = new Mock<IDirectory>();
            fileMock = new Mock<IFile>();
            baseOperator = new BaseOperator(fileSystemMock.Object, dataProviderMock.Object);
        }

        [TestMethod]
        public void WriteTreeTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] { @".\hello.txt" });
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { @".\sub", @".\.ugit" });
            directoryMock.Setup(d => d.EnumerateFiles(@".\sub")).Returns(new[]
            {
                @".\sub\ugit.txt"
            });
            byte[] helloData = Encoding.UTF8.GetBytes("Hello World");
            byte[] ugitData = Encoding.UTF8.GetBytes("Hello Ugit");
            fileMock.Setup(f => f.ReadAllBytes(@".\hello.txt")).Returns(helloData);
            fileMock.Setup(f => f.ReadAllBytes(@".\sub\ugit.txt")).Returns(ugitData);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProviderMock.Setup(f => f.GitDir).Returns(".ugit");
            string subStree = "blob bar ugit.txt";

            string tree = string.Join("\n", new string[]
            {
                $"blob foo hello.txt",
                $"tree baz sub"
            });

            dataProviderMock.Setup(f => f.HashObject(It.IsAny<byte[]>(), It.IsAny<string>())).Returns<byte[], string>((data, type)=>
            {
                if (data.SequenceEqual(Encoding.UTF8.GetBytes(subStree)) && type =="tree")
                {
                    return "baz";
                }

                if (data.SequenceEqual(Encoding.UTF8.GetBytes(tree)) && type == "tree")
                {
                    return "foobar";
                }

                if (data.SequenceEqual(helloData) && type =="blob")
                {
                    return "foo";
                }

                if(data.SequenceEqual(ugitData) && type == "blob")
                {
                    return "bar";
                }
                return null;
            });
            string expected = "foobar";
            Assert.AreEqual(expected, baseOperator.WriteTree());
            fileMock.VerifyAll();
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
            dataProviderMock.VerifyAll();
        }
    }
}
