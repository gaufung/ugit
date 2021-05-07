namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// Default implementation of IRemoteOperation.
    /// </summary>
    public class DefaultRemoteOperation : IRemoteOperation
    {
        private static readonly string RemoteRefsBase = Path.Join(Constants.Refs, Constants.Heads);
        private static readonly string LocalRefsBase = Path.Join(Constants.Refs, Constants.Remote);
        private readonly IDataProvider localDataProvider;
        private readonly ICommitOperation localCommitOperation;
        private readonly IDataProvider remoteDataProvider;
        private readonly ICommitOperation remoteCommitOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultRemoteOperation"/> class.
        /// </summary>
        /// <param name="localDataProvider">local data provider.</param>
        /// <param name="localCommitOperation">local tree operation.</param>
        /// <param name="remoteDataProvider">remote data provider.</param>
        /// <param name="remoteCommitOperation">remote commit operation.</param>
        public DefaultRemoteOperation(
            IDataProvider localDataProvider,
            ICommitOperation localCommitOperation,
            IDataProvider remoteDataProvider,
            ICommitOperation remoteCommitOperation)
        {
            this.localDataProvider = localDataProvider;
            this.localCommitOperation = localCommitOperation;
            this.remoteDataProvider = remoteDataProvider;
            this.remoteCommitOperation = remoteCommitOperation;
        }

        /// <inheritdoc/>
        public void Fetch()
        {
            var refs = this.remoteDataProvider.GetRefsMapping(RemoteRefsBase);
            foreach (var oid in this.remoteCommitOperation.GetObjectHistory(refs.Values))
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
            HashSet<string> remoteObjects = new HashSet<string>(this.localCommitOperation.GetObjectHistory(knowRemoteRefs));
            HashSet<string> localObjects = new HashSet<string>(this.localCommitOperation.GetObjectHistory(new[] { localRef }));
            IEnumerable<string> objectsToPush = localObjects.Except(remoteObjects);
            foreach (var oid in objectsToPush)
            {
                this.PushObject(oid);
            }

            this.remoteDataProvider.UpdateRef(refName, RefValue.Create(false, localRef));
        }

        private void FetchObjectIfMissing(string oid)
        {
            if (this.localDataProvider.ObjectExist(oid))
            {
                return;
            }

            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, Constants.Objects, oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, Constants.Objects, oid);
            byte[] bytes = this.remoteDataProvider.Read(remotePath);
            this.localDataProvider.Write(localPath, bytes);
        }

        private void PushObject(string oid)
        {
            string localPath = Path.Join(this.localDataProvider.GitDirFullPath, Constants.Objects, oid);
            string remotePath = Path.Join(this.remoteDataProvider.GitDirFullPath, Constants.Objects, oid);
            byte[] bytes = this.localDataProvider.Read(localPath);
            this.remoteDataProvider.Write(remotePath, bytes);
        }

        private bool IsAncestorOf(string commit, string maybeAncestor)
        {
            return this.localCommitOperation.GetHistory(new[] { commit }).Contains(maybeAncestor);
        }
    }
}