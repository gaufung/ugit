using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NuGet.Frameworks;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using Tindo.UgitCore.Operations;
using Tindo.UgitCore;

namespace Ugit
{
    [TestClass]
    public class ExtensionTest
    {
        [TestMethod]
        public void Sha1HexDigestTest()
        {
            byte[] data = "Hello World".Encode();
            string expected = "0a4d55a8d778e5022fab701977c5d840bbc486d0";
            Assert.AreEqual(expected, data.Sha1HexDigest());
        }

        [TestMethod]
        public void EncodeTest()
        {
            string str = "Hello World";
            byte[] expected = new byte[] { 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100 };
            CollectionAssert.AreEqual(expected, str.Encode());
        }

        [TestMethod]
        public void DecodeTest()
        {
            byte[] data = new byte[] { 72, 101, 108, 108, 111, 32, 87, 111, 114, 108, 100 };
            string expected = "Hello World";
            Assert.AreEqual(expected, data.Decode());
        }

        [TestMethod]
        public void UpdateTest()
        {
            IDictionary<string, string> a = new Dictionary<string, string>()
            {
                {"foo", "foobar" }
            };
            IDictionary<string, string> b = new Dictionary<string, string>()
            {
                {"foo", "foobaz" },
                {"bar", "barbaz" }
            };
            a.Update(b);
            Assert.AreEqual(2, a.Count);
            Assert.AreEqual("foobaz", a["foo"]);
            Assert.AreEqual("barbaz", a["bar"]);
        }

        [TestMethod]
        public void CreateParentDictoryTest()
        {
            string filePath = Path.Join("foo", "bar", "hello.txt");
            Mock<IDirectory> directoryMock = new Mock<IDirectory>();
            string parentPath = Path.Join("foo", "bar");
            directoryMock.Setup(d => d.Exists(parentPath)).Returns(false);
            directoryMock.Setup(d => d.CreateDirectory(parentPath));
            Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Object.CreateParentDirectory(filePath);
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void CreateParentDictoryExistTest()
        {
            string filePath = Path.Join("foo", "bar", "hello.txt");
            Mock<IDirectory> directoryMock = new Mock<IDirectory>();
            string parentPath = Path.Join("foo", "bar");
            directoryMock.Setup(d => d.Exists(parentPath)).Returns(true);
            Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileSystemMock.Object.CreateParentDirectory(filePath);
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void CreateParentDictoryEmptyTest()
        {
            string filePath = Path.Join("hello.txt");
            Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Object.CreateParentDirectory(filePath);
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void IsOnlyHexTest()
        {
            string str = null;
            Assert.IsFalse(str.IsOnlyHex());
            str = "0a4d55a8d778e5022fab701977c5d840bbc486d0";
            Assert.IsTrue(str.IsOnlyHex());
        }

        [TestMethod]
        public void FileSystemWalkTest()
        {
            Mock<IDirectory> directoryMock = new Mock<IDirectory>();
            string directory = ".ugit";
            directoryMock.Setup(d => d.EnumerateFiles(".ugit")).Returns(new string[]
            {
                Path.Join(".ugit", "hello.txt")
            });

            directoryMock.Setup(d => d.EnumerateDirectories(".ugit")).Returns(new string[] 
            {
                Path.Join(".ugit", "sub")
            });

            directoryMock.Setup(d => d.EnumerateFiles(Path.Join(".ugit", "sub"))).Returns(new string[]
            {
                Path.Join(".ugit", "sub", "ugit.txt")
            });
            directoryMock.Setup(d => d.Exists(It.IsAny<string>())).Returns(true);
            Mock<IFileSystem> fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(s => s.Directory).Returns(directoryMock.Object);
            string[] filePaths = fileSystemMock.Object.Walk(directory).ToArray();
            CollectionAssert.AreEqual(new string[]
            {
                Path.Join(".ugit", "hello.txt"),
                Path.Join(".ugit", "sub", "ugit.txt")
            }, filePaths);
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }
    }
}
