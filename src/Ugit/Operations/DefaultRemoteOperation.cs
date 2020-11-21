namespace Ugit
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Security.Cryptography;
    using Ugit.Operations;

    internal class DefaultRemoteOperation : IRemoteOperation
    {
        private static readonly string RemoteRefsBase = Path.Join("refs", "heads");

        private static readonly string LocalRefsBase = Path.Join("refs", "remote");

        private readonly IDataProvider remoteDataProvider;

        private readonly IDataProvider localDataProvider;

        private readonly ICommitOperation remoteCommitOperation;

        private readonly ITreeOperation remoteTreeOperation;

        public DefaultRemoteOperation(IDataProvider remoteDataProvider, IDataProvider localDataProvider, ITreeOperation remoteTreeOperation = null, ICommitOperation remoteCommitOperation = null)
        {
            this.remoteDataProvider = remoteDataProvider;
            this.localDataProvider = localDataProvider;
            if (remoteTreeOperation == null)
            {
                this.remoteTreeOperation = new DefaultTreeOperation(this.remoteDataProvider);
            }
            if (remoteCommitOperation == null)
            {
                this.remoteCommitOperation = new DefaultCommitOperation(
                    this.remoteDataProvider, this.remoteTreeOperation);
            }
        }

        public void Fetch()
        {
            Debugger.Launch();
            var refs = this.GetRemoteRefs(RemoteRefsBase);
            foreach (var oid in this.IterObjectsInCommits(refs.Values))
            {
                this.FetchObjectIfMissing(oid);
            }

            foreach (var entry in refs)
            {
                string remoteName = entry.Key;
                string value = entry.Value;
                string refName = Path.GetRelativePath(RemoteRefsBase, remoteName);
                this.localDataProvider.UpdateRef(
                    Path.Join(LocalRefsBase, refName),
                    RefValue.Create(false, value));
            }
        }

        private IEnumerable<string> IterObjectsInCommits(IEnumerable<string> oids)
        {
            //Debugger.Launch();
            HashSet<string> visited = new HashSet<string>();

            IEnumerable<string> IterObjectsInTree(string oid)
            {
                visited.Add(oid);
                yield return oid;

                foreach (var (type, subOid, _) in this.remoteTreeOperation.IterTreeEntry(oid))
                {
                    if (!visited.Contains(subOid))
                    {
                        if (type == "tree")
                        {
                            foreach (var val in IterObjectsInTree(subOid))
                            {
                                yield return val;
                            }
                        }
                        else
                        {
                            visited.Add(subOid);
                            yield return subOid;
                        }
                    }
                }
            }

            foreach (var oid in this.remoteCommitOperation.GetCommitHistory(oids))
            {
                yield return oid;
                var commit = this.remoteCommitOperation.GetCommit(oid);
                if (!visited.Contains(commit.Tree))
                {
                    foreach (var val in IterObjectsInTree(commit.Tree))
                    {
                        yield return val;
                    }
                }
            }
        }

        private IDictionary<string, string> GetRemoteRefs(string prefix)
        {
            IDictionary<string, string> refs = new Dictionary<string, string>();
            foreach (var (refname, @ref) in this.remoteDataProvider.GetAllRefs(prefix))
            {
                refs.Add(refname, @ref.Value);
            }

            return refs;
        }

        private void FetchObjectIfMissing(string oid)
        {
            if (this.localDataProvider.Exist(oid))
            {
                return;
            }

            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, "objects", oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, "objects", oid);
            byte[] bytes = this.remoteDataProvider.ReadAllBytes(remotePath);
            this.localDataProvider.WriteAllBytes(localPath, bytes);
        }
    }
}
