namespace Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using Ugit.Operations;

    [TestClass]
    public class DefaultRemoteOperationTests
    {
        private Mock<IDataProvider> localDataProviderMock = new();
        private Mock<ITreeOperation> localTreeOperationMock = new();
        private Mock<ICommitOperation> localCommitOperationMock = new();
        private Mock<IDataProvider> remoteDataProviderMock = new();
        private Mock<ICommitOperation> remoteCommitOperationMock = new();
        private Mock<ITreeOperation> remoteTreeOperationMock = new();
        

        private IRemoteOperation remoteOpetaion;

        [TestInitialize]
        public void Init()
        {
            remoteOpetaion = new DefaultRemoteOperation(
                localDataProviderMock.Object,
                localTreeOperationMock.Object,
                localCommitOperationMock.Object,
                remoteDataProviderMock.Object,
                remoteTreeOperationMock.Object,
                remoteCommitOperationMock.Object
            );
        }

        [TestMethod]
        public void FetchTest()
        {
            this.remoteDataProviderMock.Setup(r => r.GetAllRefs(Path.Join("refs", "heads"), true)).Returns(
                new []{
                    (Path.Join("refs", "heads", "master"), RefValue.Create(false, "second-commit-oid"))
                }
            );

            this.remoteCommitOperationMock.Setup(r => r.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>
            {
                if (oids != null && oids.Count() == 1 && oids.First() == "second-commit-oid")
                {
                    return new [] {
                        "second-commit-oid",
                        "first-commit-oid"
                    };
                }
                return Array.Empty<string>();
            });

            this.remoteCommitOperationMock.Setup(r =>r.GetCommit(It.IsAny<string>())).Returns<string>(oid => 
            {
                if (oid == "second-commit-oid")
                {
                    return new Commit{
                        Tree = "second-tree-oid"
                    };
                }

                if (oid == "first-commit-oid")
                {
                    return new Commit{
                        Tree = "first-tree-oid"
                    };
                }

                throw new Exception("unexpected commit oid.");
            });

            this.remoteTreeOperationMock.Setup(r => r.IterTreeEntry(It.IsAny<string>())).Returns<string>(oid => {
                if (oid == "second-tree-oid") 
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                        ("tree", "sub-folder-oid", "sub"),
                    };
                }

                if (oid == "sub-folder-oid")
                {
                    return new [] {
                        ("blob", "world-oid", "world.txt")
                    };
                }
                
                if (oid == "first-commit-oid")
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                    };
                }

                if (oid == "first-tree-oid")
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                    };
                }

                throw new Exception($"unknow object id: {oid}");
            });

            this.localDataProviderMock.Setup(l => l.ObjectExist(It.IsAny<string>())).Returns<string>(oid => {
                if (oid == "hello-oid" || oid == "world-oid" || oid == "sub-folder-oid" 
                || oid == "second-commit-oid" || oid == "first-commit-oid" || oid == "second-tree-oid"
                || oid == "first-tree-oid") 
                {
                    return false;
                }

                throw new Exception($"unknow object id: {oid}");
            });

            this.localDataProviderMock.Setup(l => l.GitDirFullPath).Returns(Path.Join("local", "repo", ".ugit"));
            this.remoteDataProviderMock.Setup(l => l.GitDirFullPath).Returns(Path.Join("remote", "repo", ".ugit"));
            this.remoteDataProviderMock.Setup(r => r.Read(It.IsAny<string>())).Returns<string>(path => {
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "hello-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "world-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "sub-folder-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "second-commit-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "first-commit-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "second-tree-oid"))
                {
                    return Array.Empty<byte>();
                }
                if (path == Path.Join("remote", "repo", ".ugit", "objects", "first-tree-oid"))
                {
                    return Array.Empty<byte>();
                }
                throw new Exception($"unknow path: {path}");
            });
            this.localDataProviderMock.Setup(l => l.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            this.localDataProviderMock.Setup(l => l.UpdateRef(Path.Join("refs", "remote", "master"), It.IsAny<RefValue>(), true));

            remoteOpetaion.Fetch();

            this.localDataProviderMock.VerifyAll();
            this.remoteDataProviderMock.VerifyAll();
            this.remoteCommitOperationMock.VerifyAll();
            this.remoteTreeOperationMock.VerifyAll();
        }

        [TestMethod]
        [ExpectedException(typeof(UgitException))]
        public void PushFailureTest()
        {
            this.remoteDataProviderMock.Setup(r => r.GetAllRefs(string.Empty, true)).Returns(
                new []{
                    (Path.Join("refs", "heads", "master"), RefValue.Create(false, "second-commit-oid"))
                }
            );

            string refName = Path.Join("refs", "heads", "master");
            this.localDataProviderMock.Setup(l => l.GetRef(refName, true)).Returns(RefValue.Create(false, "first-commit-oid"));
            this.localCommitOperationMock.Setup(l => l.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>{
                if (oids != null && oids.Count() == 1 && oids.First() == "first-commit-oid")
                {
                    return Array.Empty<string>();
                }
                throw new Exception($"unknown local ref");
            });
            remoteOpetaion.Push(refName);
            this.remoteDataProviderMock.VerifyAll();
            this.localDataProviderMock.VerifyAll();
            this.localCommitOperationMock.VerifyAll();
        }

        [TestMethod]
        public void PushTest()
        {
            this.remoteDataProviderMock.Setup(r => r.GetAllRefs(string.Empty, true)).Returns(
                new []{
                    (Path.Join("refs", "heads", "master"), RefValue.Create(false, "first-commit-oid"))
                }
            );

            string refName = Path.Join("refs", "heads", "master");
            this.localDataProviderMock.Setup(l => l.GetRef(refName, true)).Returns(RefValue.Create(false, "second-commit-oid"));
            this.localDataProviderMock.Setup(l => l.ObjectExist("first-commit-oid")).Returns(true);
            this.localCommitOperationMock.Setup(l => l.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>{
                if (oids != null && oids.Count() == 1 && oids.First() == "second-commit-oid")
                {
                    return new [] {"second-commit-oid", "first-commit-oid"};
                }
                throw new Exception($"unknown local ref");
            });

            this.localCommitOperationMock.Setup(r => r.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>
            {
                if (oids != null && oids.Count() == 1 && oids.First() == "second-commit-oid")
                {
                    return new [] {
                        "second-commit-oid",
                        "first-commit-oid"
                    };
                }

                if (oids!=null && oids.Count() == 1 && oids.First() == "first-commit-oid")
                {
                    return new [] {
                        "first-commit-oid"
                    };
                }
                return Array.Empty<string>();
            });

            this.localCommitOperationMock.Setup(r =>r.GetCommit(It.IsAny<string>())).Returns<string>(oid => 
            {
                if (oid == "second-commit-oid")
                {
                    return new Commit{
                        Tree = "second-tree-oid"
                    };
                }

                if (oid == "first-commit-oid")
                {
                    return new Commit{
                        Tree = "first-tree-oid"
                    };
                }

                throw new Exception("unexpected commit oid.");
            });

            this.localTreeOperationMock.Setup(r => r.IterTreeEntry(It.IsAny<string>())).Returns<string>(oid => {
                if (oid == "second-tree-oid") 
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                        ("tree", "sub-folder-oid", "sub"),
                    };
                }

                if (oid == "sub-folder-oid")
                {
                    return new [] {
                        ("blob", "world-oid", "world.txt")
                    };
                }
                
                if (oid == "first-commit-oid")
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                    };
                }

                if (oid == "first-tree-oid")
                {
                    return new [] {
                        ("blob", "hello-oid", "hello.txt"),
                    };
                }

                throw new Exception($"unknow object id: {oid}");
            });
            this.localDataProviderMock.Setup(l => l.GitDirFullPath).Returns(Path.Join("local", "repo", ".ugit"));
            this.remoteDataProviderMock.Setup(l => l.GitDirFullPath).Returns(Path.Join("remote", "repo", ".ugit"));
            this.localDataProviderMock.Setup(l => l.Read(It.IsAny<string>())).Returns(Array.Empty<byte>());
            this.remoteDataProviderMock.Setup(r => r.Write(It.IsAny<string>(), It.IsAny<byte[]>()));
            this.remoteDataProviderMock.Setup(r => r.UpdateRef(refName, It.IsAny<RefValue>(), true));
            remoteOpetaion.Push(refName);
            
            this.localDataProviderMock.VerifyAll();
            this.remoteDataProviderMock.VerifyAll();
            this.localTreeOperationMock.VerifyAll();
            this.localCommitOperationMock.VerifyAll();
        }
    }
}