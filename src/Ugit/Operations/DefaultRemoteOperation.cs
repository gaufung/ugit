namespace Ugit
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Ugit.Operations;

    internal class DefaultRemoteOperation : IRemoteOperation
    {
        private static readonly string RemoteRefsBase = Path.Join("refs", "heads");

        private static readonly string LocalRefsBase = Path.Join("refs", "remote");

        private readonly IDataProvider remoteDataProvider;

        private readonly IDataProvider localDataProvider;

        private readonly ITreeOperation localTreeOperation;

        private readonly ICommitOperation localCommitOperation;

        private readonly ICommitOperation remoteCommitOperation;

        private readonly ITreeOperation remoteTreeOperation;

        public DefaultRemoteOperation(IDataProvider remoteDataProvider, IDataProvider localDataProvider, ITreeOperation remoteTreeOperation = null, ICommitOperation remoteCommitOperation = null, ITreeOperation localTreeOperation = null, ICommitOperation localCommitOpeartion = null)
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

            if (localTreeOperation == null)
            {
                this.localTreeOperation = new DefaultTreeOperation(this.localDataProvider);
            }

            if (localCommitOpeartion == null)
            {
                this.localCommitOperation = new DefaultCommitOperation(this.localDataProvider, this.localTreeOperation);
            }
        }

        public void Fetch()
        {
            var refs = this.GetRemoteRefs(RemoteRefsBase);
            foreach (var oid in this.IterObjectsInCommits(this.remoteTreeOperation, this.remoteCommitOperation, refs.Values))
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

        private IEnumerable<string> IterObjectsInCommits(ITreeOperation treeOpeation, ICommitOperation commitOpeartion, IEnumerable<string> oids)
        {
            HashSet<string> visited = new HashSet<string>();
            IEnumerable<string> IterObjectsInTree(string oid)
            {
                visited.Add(oid);
                yield return oid;

                foreach (var (type, subOid, _) in treeOpeation.IterTreeEntry(oid))
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

            foreach (var oid in commitOpeartion.GetCommitHistory(oids))
            {
                yield return oid;
                var commit = commitOpeartion.GetCommit(oid);
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

        public void Push(string refName)
        {
            //Debugger.Launch();
            var remoteRefs = this.GetRemoteRefs(string.Empty);
            string remoteRef = string.Empty;
            remoteRefs.TryGetValue(refName, out remoteRef);
            string localRef = this.localDataProvider.GetRef(refName).Value;
            if (!string.IsNullOrEmpty(remoteRef) && !this.isAncestorOf(localRef, remoteRef))
            {
                throw new UgitException("Could not push");
            }

            IEnumerable<string> knowRemoteRefs = remoteRefs.Values.Where(this.localDataProvider.ObjectExist);
            HashSet<string> remoteObjects = new HashSet<string>(this.IterObjectsInCommits(this.localTreeOperation, this.localCommitOperation, knowRemoteRefs));
            HashSet<string> localObjects = new HashSet<string>(this.IterObjectsInCommits(this.localTreeOperation, this.localCommitOperation, new[] { localRef }));
            IEnumerable<string> objectsToPush = localObjects.Except(remoteObjects);
            foreach (var oid in objectsToPush)
            {
                this.PushObject(oid);
            }

            this.remoteDataProvider.UpdateRef(refName, RefValue.Create(false, localRef));
        }

        private void PushObject(string oid)
        {
            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, "objects", oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, "objects", oid);
            byte[] bytes = this.localDataProvider.ReadAllBytes(localPath);
            this.remoteDataProvider.WriteAllBytes(remotePath, bytes);
        }

        private bool isAncestorOf(string commit, string maybeAncestor)
        {
            return this.localCommitOperation.GetCommitHistory(new[] { commit }).Contains(maybeAncestor);
        }
    }
}
