namespace Tindo.Ugit
{
    using FluentAssertions;
    using Moq;
    using System.IO.Abstractions;
    using System;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    
    [TestClass]
    public class PhysicalFileOperatorTests
    {
        private Mock<IFileSystem> fileSystemMock;
        private Mock<IFile> fileMock;
        private Mock<IDirectory> directoryMock;

        private IFileOperator fileOperator;

        [TestInitialize]
        public void Init()
        {
            fileSystemMock = new Mock<IFileSystem>(MockBehavior.Strict);
            fileMock = new Mock<IFile>(MockBehavior.Strict);
            directoryMock = new Mock<IDirectory>(MockBehavior.Strict);
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            fileOperator = new PhysicalFileOperator(fileSystemMock.Object);
        }

        [TestMethod]
        public void DeleteTest()
        {
            string path = "foo.txt";
            fileMock.Setup(f => f.Exists(path)).Returns(true);
            fileMock.Setup(f => f.Delete(path));
            fileOperator.Delete(path);
            fileMock.VerifyAll();

            path = "bar";
            directoryMock.Setup(d => d.Exists(path)).Returns(true);
            directoryMock.Setup(d => d.Delete(path, true));
            fileOperator.Delete(path, false);
            directoryMock.VerifyAll();
        }

        [TestMethod]
        public void EmptyCurrentDirectoryTest()
        {
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[]
            {
                "readme.txt",
            });

            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[]
            {
                "foo/",
                ".ugit/"
            });
            fileMock.Setup(f => f.Exists("readme.txt")).Returns(true);
            fileMock.Setup(f => f.Delete("readme.txt"));

            directoryMock.Setup(d => d.Exists("foo/")).Returns(true);
            directoryMock.Setup(d => d.Delete("foo/", true));
            
            fileOperator.EmptyCurrentDirectory((path)=>path.StartsWith(".ugit"));
            
            fileMock.VerifyAll();
            directoryMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }

        [TestMethod]
        public void TryReadTest()
        {
            string path = "foo.txt";
            fileMock.Setup(f => f.Exists(path)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(path)).Returns("hello world".Encode());

            var result = fileOperator.TryRead(path, out var bytes);
            result.Should().BeTrue();
            bytes.Should().BeEquivalentTo("hello world".Encode());
            fileMock.VerifyAll();

            path = "bar.txt";
            fileMock.Setup(f => f.Exists(path)).Returns(false);

            result = fileOperator.TryRead(path, out bytes);
            result.Should().BeFalse();
            bytes.Should().BeEquivalentTo(Array.Empty<byte>());
        }

        [TestMethod]
        public void ReadTest()
        {
            string path = "foo.txt";
            fileMock.Setup(f => f.Exists(path)).Returns(true);
            fileMock.Setup(f => f.ReadAllBytes(path)).Returns("hello world".Encode());

            var result = fileOperator.Read(path);
            result.Should().BeEquivalentTo("hello world".Encode());
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void ReadFailureTest()
        {
            var path = "bar.txt";
            fileMock.Setup(f => f.Exists(path)).Returns(false);
            fileOperator.Read(path);
        }
        
    }
}