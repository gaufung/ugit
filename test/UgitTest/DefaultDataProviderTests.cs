using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text;

namespace Ugit
{
    [TestClass]
    [Ignore]
    public class DefaultDataProviderTests
    {
        private Mock<IDirectory> direcotryMock;

        private Mock<IFileSystem> fileSystemMock;

        private Mock<IFile> fileMock;

        private IDataProvider dataProvider;

        private Mock<IFileOperator> fileOperator;

        [TestInitialize]
        public void Init()
        {
            direcotryMock = new Mock<IDirectory>(MockBehavior.Loose);
            fileMock = new Mock<IFile>(MockBehavior.Loose);
            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Loose);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            fileOperator = new Mock<IFileOperator>();
            dataProvider = new LocalDataProvider(fileOperator.Object);
        }

        [TestMethod]
        public void InitTest()
        {
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(false);
            direcotryMock.Setup(d => d.CreateDirectory(".ugit"));
            string directoryPath = Path.Join(".ugit", "objects");
            direcotryMock.Setup(d => d.CreateDirectory(directoryPath));
            dataProvider.Init();
            direcotryMock.VerifyAll();
        }

        [TestMethod]
        public void InitTwiceTest()
        {
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            direcotryMock.Setup(d => d.Delete(".ugit", true));
            direcotryMock.Setup(d => d.CreateDirectory(".ugit"));
            string directoryPath = Path.Combine(".ugit", "objects");
            direcotryMock.Setup(d => d.CreateDirectory(directoryPath));
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.Init();
            direcotryMock.VerifyAll();
        }

        [TestMethod]
        public void GitDirAndPathTest()
        {
            Assert.AreEqual(".ugit", dataProvider.GitDir);
            Assert.AreEqual(".ugit", dataProvider.GitDirFullPath);
        }

        [TestMethod]
        public void HashObjectTest()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello World");
            string filePath = Path.Join(".ugit", "objects", "0a6649a0077da1bf5a8b3b5dd3ea733ea6a81938");
            fileMock.Setup(f => f.WriteAllBytes(filePath, data));
            var actual = dataProvider.HashObject(data);
            Assert.AreEqual("0a6649a0077da1bf5a8b3b5dd3ea733ea6a81938", actual);
        }

        [TestMethod]
        public void HashObjectEmptyTypeTest()
        {
            byte[] data = Encoding.UTF8.GetBytes("Hello World");
            string filePath = Path.Join(".ugit", "objects", "0a4d55a8d778e5022fab701977c5d840bbc486d0");
            fileMock.Setup(f => f.WriteAllBytes(filePath, data));
            var actual = dataProvider.HashObject(data, "");
            Assert.AreEqual("0a4d55a8d778e5022fab701977c5d840bbc486d0", actual);
        }

        [TestMethod]
        public void GetObjectNonExistTest()
        {
            string oid = "foo";
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(false);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            var expected = Array.Empty<byte>();
            CollectionAssert.AreEqual(expected, dataProvider.GetObject(oid));
        }

        [TestMethod]
        public void GetObjectExistTest()
        {
            string oid = "foo";
            byte[] data = "Hello World".Encode();
            data = "blob".Encode().Concat(new[] { byte.Parse("0") }).Concat(data).ToArray();
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(data);
            var expected = "Hello World".Encode();
            CollectionAssert.AreEqual(expected, dataProvider.GetObject(oid));
        }

        [TestMethod]
        public void GetObjectWithoutTypeTest()
        {
            string oid = "foo";
            byte[] data = "Hello World".Encode();
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(data);
            var actual = dataProvider.GetObject(oid);
            CollectionAssert.AreEqual(Array.Empty<byte>(), actual);
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void GetObjectUnexpectedTypeTest()
        {
            string oid = "foo";
            byte[] data = "Hello World".Encode();
            data = "unknow".Encode().Concat(new[] { byte.Parse("0") }).Concat(data).ToArray();
            string filePath = Path.Join(".ugit", "objects", oid);
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(data);
            dataProvider.GetObject(oid);
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void UpdateRefTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            string oid = "foo";
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(filePath, It.IsAny<byte[]>()));
            dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void UpdateRefTrueTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            string @ref = Path.Join(".ugit", "heads", "master");
            direcotryMock.Setup(d => d.Exists(".ugit")).Returns(true);
            fileMock.Setup(f => f.WriteAllBytes(filePath, It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            dataProvider.UpdateRef("HEAD", RefValue.Create(true, @ref));
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void GetRefNullTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(false);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.IsNull(dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void GetRefTest()
        {
            string filePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(filePath)).Returns(true);
            string head = "Hello World";
            fileMock.Setup(f => f.ReadAllBytes(filePath)).Returns(head.Encode());
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.AreEqual(head, dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void GeRefDerefTest()
        {
            string headFilePath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(headFilePath)).Returns(true);
            string head = "ref:master";
            fileMock.Setup(f => f.ReadAllBytes(headFilePath)).Returns(head.Encode());
            string masterFilePath = Path.Join(".ugit", "master");
            fileMock.Setup(f => f.Exists(masterFilePath)).Returns(true);
            string master = "foo";
            fileMock.Setup(f => f.ReadAllBytes(masterFilePath)).Returns(master.Encode());
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            Assert.AreEqual("foo", dataProvider.GetRef("HEAD").Value);
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void GetAllRefsTest()
        {
            string headPath = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(headPath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(headPath)).Returns("foo".Encode());

            string tagPath = Path.Join(".ugit", "refs", "tags", "v1.0");
            fileMock.Setup(f => f.Exists(tagPath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(tagPath)).Returns("bar".Encode());

            direcotryMock.Setup(d => d.EnumerateFiles(Path.Join(".ugit", "refs"))).Returns(Array.Empty<string>());
            direcotryMock.Setup(d => d.EnumerateDirectories(Path.Join(".ugit", "refs"))).Returns(new string[]
            {
                Path.Join(".ugit", "refs", "tags")
            });

            direcotryMock.Setup(d => d.EnumerateFiles(Path.Join(".ugit", "refs", "tags"))).Returns(new string[]
            {
                Path.Join(".ugit", "refs", "tags", "v1.0")
            });
            direcotryMock.Setup(d => d.Exists(It.IsAny<string>())).Returns(true);
            fileSystemMock.Setup(f => f.Directory).Returns(direcotryMock.Object);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            (string, RefValue)[] refs = dataProvider.GetAllRefs().ToArray();
            CollectionAssert.AreEqual(new (string, RefValue)[]
            {
                ValueTuple.Create("HEAD", RefValue.Create(false, "foo")),
                ValueTuple.Create(Path.Join("refs", "tags", "v1.0"), RefValue.Create(false, "bar"))
            }, refs);
        }

        [TestMethod]
        public void DeleteRefTest()
        {
            string @ref = Path.Join("refs", "head", "dev");
            fileMock.Setup(f => f.Exists(Path.Join(".ugit", @ref))).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(Path.Join(".ugit", @ref))).Returns("foo".Encode());
            fileMock.Setup(f => f.Delete(Path.Join(".ugit", @ref)));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            dataProvider.DeleteRef(@ref);
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void GetIndexEmptyTest()
        {
            string indexPath = Path.Join(".ugit", "index");
            fileMock.Setup(f => f.Exists(indexPath)).Returns(false);
            var index = dataProvider.Index;
            Assert.AreEqual(0, index.Count);
        }

        [TestMethod]
        public void GetIndexTest()
        {
            string indexPath = Path.Join(".ugit", "index");
            fileMock.Setup(f => f.Exists(indexPath)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(indexPath)).Returns("{\"foo\": \"bar\"}".Encode());
            var index = dataProvider.Index;
            Assert.AreEqual(1, index.Count);
            Assert.IsTrue(index.ContainsKey("foo"));
            Assert.AreEqual("bar", index["foo"]);
        }

        [TestMethod]
        public void SetIndexTest()
        {
            string indexPath = Path.Join(".ugit", "index");
            fileMock.Setup(f => f.Exists(indexPath)).Returns(true);
            fileMock.Setup(f => f.Delete(indexPath));
            string data = "{\"foo\":\"bar\"}";
            fileMock.Setup(f => f.WriteAllText(indexPath, data));
            dataProvider.Index = new System.Collections.Generic.Dictionary<string, string>()
            {
                { "foo", "bar"}
            };
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void IsIgnoreTrueTest()
        {
            string path = Path.Join(".ugit", "objects", "foo");
            var actual = dataProvider.IsIgnore(path);
            Assert.IsTrue(actual);
        }

        [TestMethod]
        public void IsIgnoreFalseTest()
        {
            string path = Path.Join("sub", "objects", "foo");
            var actual = dataProvider.IsIgnore(path);
            Assert.IsFalse(actual);
        }

        [TestMethod]
        public void EmptyCurrentDirectoryTest()
        {
            direcotryMock.Setup(d => d.EnumerateFiles(".")).Returns(new string[]
            {
                Path.Join("foo.txt"),
                Path.Join(".ugit", "bar.txt")
            });
            direcotryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new string[]
            {
                Path.Join("sub"),
                Path.Join(".ugit")
            });

            fileMock.Setup(f => f.Delete(Path.Join("foo.txt")));
            direcotryMock.Setup(d => d.Delete("sub", true));
            fileOperator.Object.EmptyCurrentDirectory(dataProvider.IsIgnore);
            fileMock.VerifyAll();
            direcotryMock.VerifyAll();
        }

        [TestMethod]
        public void GetOidHEADTest()
        {
            string @ref = Path.Join(".ugit", "HEAD");
            fileMock.Setup(f => f.Exists(@ref)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(@ref)).Returns("oid".Encode());
            Assert.AreEqual("oid", this.dataProvider.GetOid("@"));
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void GetOidTest()
        {
            string name = new string('a', 40);
            Assert.AreEqual(name, this.dataProvider.GetOid(name));
        }

        [TestMethod]
        public void GetOidNullTest()
        {
            string name = "illegalOid";
            Assert.IsNull(this.dataProvider.GetOid(name));
        }

        [TestMethod]
        public void ExistFileTest()
        {
            string path = "test";
            this.fileMock.Setup(f => f.Exists(path)).Returns(true);
            Assert.IsTrue(this.fileOperator.Object.Exist(path));
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void ExistDirectroyTest()
        {
            string directory = "test";
            this.direcotryMock.Setup(d => d.Exists(directory)).Returns(true);
            Assert.IsTrue(this.fileOperator.Object.Exist(directory, false));
            direcotryMock.VerifyAll();
        }

        [TestMethod]
        public void WriteAllBytesTest()
        {
            string path = Path.Join("sub", "hello.txt");
            direcotryMock.Setup(d => d.Exists("sub")).Returns(true);
            fileMock.Setup(d => d.WriteAllBytes(path, It.IsAny<byte[]>()));
            fileOperator.Object.Write(path, Array.Empty<byte>());
            direcotryMock.VerifyAll();
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void ReadAllBytesTest()
        {
            this.fileMock.Setup(d => d.ReadAllBytes("test.txt")).Returns(Array.Empty<byte>());
            CollectionAssert.AreEqual(Array.Empty<byte>(), fileOperator.Object.Read("test.txt"));
            fileMock.VerifyAll();
        }

        [TestMethod]
        public void DeleteIgnoreTest()
        {
            string path = Path.Join(".ugit", "HEAD");
            fileOperator.Object.Delete(path);
        }

        [TestMethod]
        public void DeleteTest()
        {
            string path = Path.Join("sub", "foo.txt");
            this.fileMock.Setup(d => d.Delete(path));
            fileOperator.Object.Delete(path);
            this.fileMock.VerifyAll();
        }

        [TestMethod]
        public void GitFullPathDirTest()
        {
            this.direcotryMock.Setup(d => d.SetCurrentDirectory(Path.Join("foo", "bar")));
            this.direcotryMock.Setup(d => d.GetCurrentDirectory()).Returns(Path.Join("foo", "bar"));
            this.dataProvider = new LocalDataProvider(this.fileOperator.Object, Path.Join("foo", "bar"));
            Assert.AreEqual(Path.Join("foo", "bar", ".ugit"), this.dataProvider.GitDirFullPath);
        }
    }
}
