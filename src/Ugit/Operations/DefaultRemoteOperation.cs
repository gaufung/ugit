namespace Ugit
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ugit.Operations;

    /// <summary>
    /// Default implementation of IRemoteOperation.
    /// </summary>
    internal class DefaultRemoteOperation : IRemoteOperation
    {
        private static readonly string RemoteRefsBase = Path.Join("refs", "heads");
        private static readonly string LocalRefsBase = Path.Join("refs", "remote");
        private readonly IDataProvider localDataProvider;
        private readonly ITreeOperation localTreeOperation;
        private readonly ICommitOperation localCommitOperation;
        private readonly IDataProvider remoteDataProvider;
        private readonly ICommitOperation remoteCommitOperation;
        private readonly ITreeOperation remoteTreeOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRemoteOperation"/> class.
        /// </summary>
        /// <param name="localDataProvider">local data provider.</param>
        /// <param name="localTreeOperation">local tree operation.</param>
        /// <param name="localCommitOpeartion">local commit operation.</param>
        /// <param name="remoteDataProvider">remote data provder.</param>
        /// <param name="remoteTreeOperation">remote tree operation.</param>
        /// <param name="remoteCommitOperation">remote commit operation.</param>
        public DefaultRemoteOperation(
            IDataProvider localDataProvider,
            ITreeOperation localTreeOperation,
            ICommitOperation localCommitOpeartion,
            IDataProvider remoteDataProvider,
            ITreeOperation remoteTreeOperation,
            ICommitOperation remoteCommitOperation)
        {
            this.localDataProvider = localDataProvider;
            this.localTreeOperation = localTreeOperation;
            this.localCommitOperation = localCommitOpeartion;
            this.remoteDataProvider = remoteDataProvider;
            this.remoteTreeOperation = remoteTreeOperation;
            this.remoteCommitOperation = remoteCommitOperation;
        }

        /// <inheritdoc/>
        public void Fetch()
        {
            var refs = this.remoteDataProvider.GetRefsMapping(RemoteRefsBase);
            foreach (var oid in this.IterObjectsInCommits(
                this.remoteTreeOperation, this.remoteCommitOperation, refs.Values))
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

        /// <inheritdoc/>
        public void Push(string refName)
        {
            var remoteRefs = this.remoteDataProvider.GetRefsMapping(string.Empty);
            remoteRefs.TryGetValue(refName, out string remoteRef);
            string localRef = this.localDataProvider.GetRef(refName).Value;
            if (!string.IsNullOrEmpty(remoteRef) && !this.IsAncestorOf(localRef, remoteRef))
            {
                throw new UgitException("Could not push");
            }

            IEnumerable<string> knowRemoteRefs = remoteRefs.Values.Where(oid => this.localDataProvider.ObjectExist(oid));
            HashSet<string> remoteObjects = new HashSet<string>(this.IterObjectsInCommits(this.localTreeOperation, this.localCommitOperation, knowRemoteRefs));
            HashSet<string> localObjects = new HashSet<string>(this.IterObjectsInCommits(this.localTreeOperation, this.localCommitOperation, new[] { localRef }));
            IEnumerable<string> objectsToPush = localObjects.Except(remoteObjects);
            foreach (var oid in objectsToPush)
            {
                this.PushObject(oid);
            }

            this.remoteDataProvider.UpdateRef(refName, RefValue.Create(false, localRef));
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

        private void FetchObjectIfMissing(string oid)
        {
            if (this.localDataProvider.ObjectExist(oid))
            {
                return;
            }

            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, "objects", oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, "objects", oid);
            byte[] bytes = this.remoteDataProvider.ReadAllBytes(remotePath);
            this.localDataProvider.WriteAllBytes(localPath, bytes);
        }

        private void PushObject(string oid)
        {
            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, "objects", oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, "objects", oid);
            byte[] bytes = this.localDataProvider.ReadAllBytes(localPath);
            this.remoteDataProvider.WriteAllBytes(remotePath, bytes);
        }

        private bool IsAncestorOf(string commit, string maybeAncestor)
        {
            return this.localCommitOperation.GetCommitHistory(new[] { commit }).Contains(maybeAncestor);
        }
    }
}
