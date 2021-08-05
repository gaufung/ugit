namespace Tindo.Ugit
{
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    [TestClass]
    public class RemoteOperationTests
    {
        private Mock<IDataProvider> localDataProviderMock = new();
        private Mock<ICommitOperation> localCommitOperationMock = new();
        private Mock<IDataProvider> remoteDataProviderMock = new();
        private Mock<ICommitOperation> remoteCommitOperationMock = new();
        private Mock<IFileOperator> localFileOperator = new();
        private Mock<IFileOperator> remoteFileOperator = new();
        private IRemoteOperation remoteOpetaion;

        [TestInitialize]
        public void Init()
        {
            remoteOpetaion = new RemoteOperation(
                localDataProviderMock.Object,
                localCommitOperationMock.Object,
                remoteDataProviderMock.Object,
                remoteCommitOperationMock.Object
            ) ;
        }

        [TestMethod]
        public void FetchTest()
        {
            this.remoteDataProviderMock.Setup(r => r.GetAllRefs(Path.Join("refs", "heads"), true)).Returns(
                new []{
                    (Path.Join("refs", "heads", "master"), RefValue.Create(false, "second-commit-oid"))
                }
            );

            this.remoteCommitOperationMock.Setup(r => r.GetObjectHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>
            {
                if (oids != null && oids.Count() == 1 && oids.First() == "second-commit-oid")
                {
                    return new [] {
                        "hello-oid",
                        "world-oid",
                        "sub-folder-oid",
                        "second-commit-oid",
                        "first-commit-oid",
                        "second-tree-oid",
                        "first-tree-oid",
                    };
                }
                return Array.Empty<string>();
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

            this.remoteDataProviderMock.Setup(r => r.ReadObject(It.IsAny<string>())).Returns<string>(oid => {
                if (oid == "hello-oid" || oid == "world-oid" || oid == "sub-folder-oid"
                || oid == "second-commit-oid" || oid == "first-commit-oid" || oid == "second-tree-oid"
                || oid == "first-tree-oid")
                {
                    return Array.Empty<byte>();
                }
                throw new Exception($"unknow path: {oid}");
            });

            this.localDataProviderMock.Setup(l => l.WriteObject(It.IsAny<string>(), It.IsAny<byte[]>()));
           
            this.localDataProviderMock.Setup(l => l.UpdateRef(Path.Join("refs", "remote", "master"), It.IsAny<RefValue>(), true));

            remoteOpetaion.Fetch();

            this.localDataProviderMock.VerifyAll();
            this.remoteDataProviderMock.VerifyAll();
            this.remoteCommitOperationMock.VerifyAll();
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
            this.localCommitOperationMock.Setup(l => l.GetObjectHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>{
                if (oids != null && oids.Count() == 1 && oids.First() == "second-commit-oid")
                {
                    return new [] {"second-commit-oid", "first-commit-oid"};
                }
                throw new Exception($"unknown local ref");
            });

            this.localCommitOperationMock.Setup(r => r.GetObjectHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids =>
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
            this.localCommitOperationMock.Setup(l => l.GetCommitHistory(It.IsAny<IEnumerable<string>>())).Returns<IEnumerable<string>>(oids => {
                if (oids!=null && oids.Count()== 1 && oids.First() == "second-commit-oid")
                {
                    return new []{
                        "second-commit-oid",
                        "first-commit-oid"
                    };
                }
                throw new Exception("Unknown object ids");
            });

            this.localDataProviderMock.Setup(l => l.ReadObject(It.IsAny<string>())).Returns(Array.Empty<byte>());
            this.remoteDataProviderMock.Setup(l => l.WriteObject(It.IsAny<string>(), It.IsAny<byte[]>()));
            this.remoteDataProviderMock.Setup(r => r.UpdateRef(refName, It.IsAny<RefValue>(), true));
            remoteOpetaion.Push(refName);
            
            this.localDataProviderMock.VerifyAll();
            this.remoteDataProviderMock.VerifyAll();
            this.localCommitOperationMock.VerifyAll();
        }
    }
}