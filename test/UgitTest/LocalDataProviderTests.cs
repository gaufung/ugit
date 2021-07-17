using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
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
            byte[] data = Array.Empty<byte>();
            mockFileOperator.Setup(f => f.TryRead(path, out data)).Returns(false);
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

        [TestMethod]
        public void InitTest()
        {
            string filePath = Path.Join("foo", ".ugit");
            this.mockFileOperator.Setup(f => f.Exists(filePath, false)).Returns(true);
            this.mockFileOperator.Setup(f => f.Delete(filePath, false));
            this.mockFileOperator.Setup(f => f.CreateDirectory(filePath, true));
            this.dataProvider.Init();
            this.mockFileOperator.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void UpdateRefThrowExceptionTest()
        {
            string @ref = Path.Join("refs", "HEAD");
            this.dataProvider.UpdateRef(@ref, new RefValue(), true);
        }

        [TestMethod]
        public void UpdateRef()
        {
            string @ref = Path.Join("HEAD");
            string refPath = Path.Join("foo", ".ugit", @ref);
            byte[] data = "foobar".Encode();
            this.mockFileOperator.Setup(f => f.TryRead(refPath, out data)).Returns(true);
            this.mockFileOperator.Setup(f => f.Write(refPath, It.IsAny<byte[]>()));
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, "foo123bar"));
            this.mockFileOperator.VerifyAll();
        }

        [TestMethod]
        public void UpdateRefDeref()
        {
            string @ref = Path.Join("HEAD");
            string headRefPath = Path.Join("foo", ".ugit", @ref);
            byte[] headData = $"ref: {Path.Join("refs", "heads", "master")}".Encode();
            string masterRefPath = Path.Join("foo", ".ugit", "refs", "heads", "master");
            byte[] masterData = "foobar123".Encode();
            this.mockFileOperator.Setup(f => f.TryRead(headRefPath, out headData)).Returns(true);
            this.mockFileOperator.Setup(f => f.TryRead(masterRefPath, out masterData)).Returns(true);
            this.mockFileOperator.Setup(f => f.Write(masterRefPath, It.IsAny<byte[]>()));
            this.dataProvider.UpdateRef(@ref, RefValue.Create(false, "foo123bar"));
            this.mockFileOperator.VerifyAll();
        }


        [TestMethod]
        public void GetRefTest()
        {
            string @ref = Path.Join("HEAD");
            string headRefPath = Path.Join("foo", ".ugit", @ref);
            byte[] headData = $"ref: {Path.Join("refs", "head", "master")}".Encode();
            string masterRefPath = Path.Join("foo", ".ugit", "refs", "head", "master");
            byte[] masterData = "foobar123".Encode();
            this.mockFileOperator.Setup(f => f.TryRead(headRefPath, out headData)).Returns(true);
            this.mockFileOperator.Setup(f => f.TryRead(masterRefPath, out masterData)).Returns(true);

            this.dataProvider.GetRef(@ref, false).Symbolic.Should().BeTrue();
            this.dataProvider.GetRef(@ref, true).Value.Should().Be("foobar123");
        }

        [TestMethod]
        public void GetAllRefsTest()
        {
            string refDirectory = Path.Join("foo", ".ugit", "refs");
            this.mockFileOperator.Setup(f => f.Walk(refDirectory)).Returns(
                new[]
                {
                    Path.Join("foo", ".ugit", "refs", "heads", "master"),
                    Path.Join("foo", ".ugit", "refs", "tags", "v1")
                });
            byte[] headData = $"ref:{Path.Join("refs", "heads", "master")}".Encode();
            byte[] masterData = "foo123bar".Encode();
            byte[] v1data = "foo".Encode();
            byte[] mergeData = Array.Empty<byte>();
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "HEAD"), out headData)).Returns(true);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "MERGE_HEAD"), out mergeData)).Returns(false);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "refs", "heads", "master"), out masterData)).Returns(true);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "refs", "tags", "v1"), out v1data)).Returns(true);
            var result = this.dataProvider.GetAllRefs("", false).ToList();
            result.Count.Should().Be(3);
            result[0].Item1.Should().Be(Path.Join("HEAD"));
            result[0].Item2.Symbolic.Should().BeTrue();
            result[0].Item2.Value.Should().Be(Path.Join("refs", "heads", "master"));

            result[1].Item1.Should().Be(Path.Join("refs", "heads", "master"));
            result[1].Item2.Symbolic.Should().BeFalse();
            result[1].Item2.Value.Should().Be("foo123bar");

            result[2].Item1.Should().Be(Path.Join("refs", "tags", "v1"));
            result[2].Item2.Symbolic.Should().BeFalse();
            result[2].Item2.Value.Should().Be("foo");
        }

        [TestMethod]
        public void DeleteRefTest()
        {
            string @ref = Path.Join("HEAD");
            string headRefPath = Path.Join("foo", ".ugit", @ref);
            byte[] headData = $"ref: {Path.Join("refs", "heads", "master")}".Encode();
            byte[] masterData = "foobar123".Encode();
            string masterRefPath = Path.Join("foo", ".ugit", "refs", "heads", "master");
            this.mockFileOperator.Setup(f => f.TryRead(headRefPath, out headData)).Returns(true);
            this.mockFileOperator.Setup(f => f.TryRead(masterRefPath, out masterData)).Returns(true);
            this.mockFileOperator.Setup(f => f.Delete(masterRefPath, true));
            this.dataProvider.DeleteRef(@ref, true);
            this.mockFileOperator.VerifyAll();
        }

        [TestMethod]
        public void GetOidTest()
        {
            string @ref = Path.Join("HEAD");
            string headRefPath = Path.Join("foo", ".ugit", @ref);
            byte[] headData = $"123bar".Encode();
            this.mockFileOperator.Setup(f => f.TryRead(headRefPath, out headData)).Returns(true);
            this.dataProvider.GetOid("@").Should().Be("123bar");
        }

        [TestMethod]
        public void GetOidShaTest()
        {
            string oid = "hello world".Encode().Sha1HexDigest();
            byte[] data = Array.Empty<byte>();
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", oid), out data)).Returns(false);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "refs", oid), out data)).Returns(false);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "refs", "tags", oid), out data)).Returns(false);
            this.mockFileOperator.Setup(f => f.TryRead(Path.Join("foo", ".ugit", "refs", "heads", oid), out data)).Returns(false);
            this.dataProvider.GetOid(oid).Should().Be(oid);
        }
    }
}