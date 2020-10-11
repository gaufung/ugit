﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
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
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] { $".{Path.DirectorySeparatorChar}hello.txt" });
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { $".{Path.DirectorySeparatorChar}sub", $".{Path.DirectorySeparatorChar}.ugit" });
            directoryMock.Setup(d => d.EnumerateFiles($".{Path.DirectorySeparatorChar}sub")).Returns(new[]
            {
                $".{Path.DirectorySeparatorChar}sub{Path.DirectorySeparatorChar}ugit.txt"
            });
            byte[] helloData = Encoding.UTF8.GetBytes("Hello World");
            byte[] ugitData = Encoding.UTF8.GetBytes("Hello Ugit");
            fileMock.Setup(f => f.ReadAllBytes($".{Path.DirectorySeparatorChar}hello.txt")).Returns(helloData);
            fileMock.Setup(f => f.ReadAllBytes($".{Path.DirectorySeparatorChar}sub{Path.DirectorySeparatorChar}ugit.txt")).Returns(ugitData);
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


        [TestMethod]
        public void ReadTreeTest()
        {
            string helloFilePath = $".{Path.DirectorySeparatorChar}hello.txt";
            string subDirectory = $".{Path.DirectorySeparatorChar}sub";
            string ugitDirectory = $".{Path.DirectorySeparatorChar}.ugit";
            directoryMock.Setup(d => d.EnumerateFiles(".")).Returns(new[] {helloFilePath});
            directoryMock.Setup(d => d.EnumerateDirectories(".")).Returns(new[] { helloFilePath, subDirectory });
            string treeOid = "foo";
            string tree = string.Join("\n", new string[]
            {
                "blob bar hello.txt",
                "tree baz sub"
            });

            string subTree = string.Join("\n", new string[]
            {
                "blob zoo ugit.txt"
            });
            dataProviderMock.Setup(d => d.GetObject(treeOid, "tree")).Returns(tree.Encode());
            dataProviderMock.Setup(d => d.GetObject("baz", "tree")).Returns(subTree.Encode());

            byte[] helloData = "Hello World".Encode();
            byte[] ugitData = "Hello Ugit".Encode();
            dataProviderMock.Setup(d => d.GetObject("bar", "blob")).Returns(helloData);
            dataProviderMock.Setup(d => d.GetObject("zoo", "blob")).Returns(ugitData);
            directoryMock.Setup(d => d.Exists(Path.Join(".", "sub"))).Returns(false);
            directoryMock.Setup(d => d.CreateDirectory(Path.Join(".", "sub")));
            fileMock.Setup(s => s.WriteAllBytes(Path.Join(".","hello.txt"), It.IsAny<byte[]>()));
            fileMock.Setup(s => s.WriteAllBytes(Path.Join(".", "sub", "ugit.txt"), It.IsAny<byte[]>()));
            fileSystemMock.Setup(f => f.File).Returns(fileMock.Object);
            fileSystemMock.Setup(f => f.Directory).Returns(directoryMock.Object);
            baseOperator.ReadTree(treeOid);
            directoryMock.VerifyAll();
            fileMock.VerifyAll();
            fileSystemMock.VerifyAll();
        }
    }
}