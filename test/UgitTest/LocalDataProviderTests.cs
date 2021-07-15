using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Tindo.Ugit;
namespace Tindo.Ugit
{
    [TestClass]
    public class LocalDataProviderTests
    {
        private Mock<IFileOperator> mockFileOperator;

        private IDataProvider dataProvider;

        [TestInitialize]
        public void Init()
        {
            mockFileOperator = new Mock<IFileOperator>(MockBehavior.Strict);
            dataProvider = new LocalDataProvider(mockFileOperator.Object, "foo");
        }

        [TestMethod]
        public void GitDirFullPathTest()
        {
            string expectFullPath = Path.Join("foo", ".ugit");
            dataProvider.GitDirFullPath.Should().Be(expectFullPath);
        }

        [TestMethod]
        public void IndexSetTest()
        {
            var index = new Tree()
            {
                {"foo.txt", "abc123edf"}
            };
            string path = Path.Join("foo", ".ugit", "index");
            mockFileOperator.Setup(f 
                => f.Exists(path, true)).Returns(true);
            mockFileOperator.Setup(f => f.Delete(path, true));
            mockFileOperator.Setup(f => f.Write(path, It.IsAny<byte[]>()));
            dataProvider.Index = index;
            mockFileOperator.VerifyAll();
        }

        [TestMethod]
        public void IndexGetEmptyTest()
        {
            string path = Path.Join("foo", ".ugit", "index");
            mockFileOperator.Setup(f => f.Exists(path, true)).Returns(false);
            dataProvider.Index.Count.Should().Be(0);
            mockFileOperator.VerifyAll();
            
        }

        [TestMethod]
        public void IndexGetTest()
        {
            var index = new Tree()
            {
                {"foo.txt", "abc123edf"}
            };
            string path = Path.Join("foo", ".ugit", "index");
            var data = JsonSerializer.SerializeToUtf8Bytes(index);
            mockFileOperator.Setup(f => f.TryRead(path, out data)).Returns(true);
            dataProvider.Index.Count.Should().Be(1);
            mockFileOperator.VerifyAll();
        }

        [TestMethod]
        public void GetObjectEmptyTest()
        {
            string oid = "xxx123yyy";
            string filePath = Path.Join("foo", ".ugit", "objects", oid);
            var data = Array.Empty<byte>();
            this.mockFileOperator.Setup(f => f.TryRead(filePath, out data)).Returns(false);
            dataProvider.GetObject(oid, "blob").Length.Should().Be(0);
        }

        [TestMethod]
        public void GetObjectSuccessTest()
        {
            string oid = "xxx123yyy";
            string filePath = Path.Join("foo", ".ugit", "objects", oid);
            var data = "blob".Encode().Concat(new byte[] {0}).Concat("hello world".Encode()).ToArray();
            this.mockFileOperator.Setup(f => f.TryRead(filePath, out data)).Returns(true);
            dataProvider.GetObject(oid, "blob").Decode().Should().Be("hello world");
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void GetObjectFailureTest()
        {
            string oid = "xxx123yyy";
            string filePath = Path.Join("foo", ".ugit", "objects", oid);
            var data = "blob".Encode().Concat(new byte[] {0}).Concat("hello world".Encode()).ToArray();
            this.mockFileOperator.Setup(f => f.TryRead(filePath, out data)).Returns(true);
            dataProvider.GetObject(oid, "commit");
        }

        [TestMethod]
        public void WriteObjectTest()
        {
            var data = "hello world".Encode();
            
            var writedata= "blob".Encode().Concat(new byte[] {0}).Concat(data).ToArray();
            string oid = writedata.Sha1HexDigest();

            string filePath = Path.Join("foo", ".ugit", "objects", oid);
            this.mockFileOperator.Setup(f => f.Write(filePath, It.IsAny<byte[]>()));
            dataProvider.WriteObject(data).Should().Be(oid);
        }
    }
}