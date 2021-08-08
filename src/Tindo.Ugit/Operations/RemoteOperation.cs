namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Default implementation of IRemoteOperation.
    /// </summary>
    internal class RemoteOperation : IRemoteOperation
    {
        private static readonly string RemoteRefsBase = Path.Join(Constants.Refs, Constants.Heads);
        private static readonly string LocalRefsBase = Path.Join(Constants.Refs, Constants.Remote);
        private readonly IDataProvider localDataProvider;
        private readonly ICommitOperation localCommitOperation;
        private readonly IDataProvider remoteDataProvider;
        private readonly ICommitOperation remoteCommitOperation;

        private readonly ILogger<RemoteOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteOperation"/> class.
        /// </summary>
        /// <param name="localDataProvider">local data provider.</param>
        /// <param name="localCommitOperation">local commit operation.</param>
        /// <param name="remoteDataProvider">remote data provider.</param>
        /// <param name="remoteCommitOperation">remote commit operation.</param>
        public RemoteOperation(
            IDataProvider localDataProvider,
            ICommitOperation localCommitOperation,
            IDataProvider remoteDataProvider,
            ICommitOperation remoteCommitOperation)
            : this(localDataProvider, localCommitOperation, remoteDataProvider, remoteCommitOperation, NullLogger<RemoteOperation>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteOperation"/> class.
        /// </summary>
        /// <param name="localDataProvider">local data provider.</param>
        /// <param name="localCommitOperation">local commit operation.</param>
        /// <param name="remoteDataProvider">remote data provider.</param>
        /// <param name="remoteCommitOperation">remote commit operation.</param>
        /// <param name="logger">The logger.</param>
        public RemoteOperation(
            IDataProvider localDataProvider,
            ICommitOperation localCommitOperation,
            IDataProvider remoteDataProvider,
            ICommitOperation remoteCommitOperation,
            ILogger<RemoteOperation> logger)
        {
            this.localDataProvider = localDataProvider;
            this.localCommitOperation = localCommitOperation;
            this.remoteDataProvider = remoteDataProvider;
            this.remoteCommitOperation = remoteCommitOperation;
            this.logger = logger;
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
                this.logger.LogWarning("Could not push");
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

            byte[] bytes = this.remoteDataProvider.ReadObject(oid);
            this.localDataProvider.WriteObject(oid, bytes);
        }

        private void PushObject(string oid)
        {
            byte[] bytes = this.localDataProvider.ReadObject(oid);
            this.remoteDataProvider.WriteObject(oid, bytes);
        }

        private bool IsAncestorOf(string commit, string maybeAncestor)
        {
            return this.localCommitOperation.GetCommitHistory(new[] { commit }).Contains(maybeAncestor);
        }
    }
}
