namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Nito.Collections;

    /// <summary>
    /// Default implementation of <see cref="ICommitOperation"/>.
    /// </summary>
    internal class CommitOperation : ICommitOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        private readonly ILogger<CommitOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data Provider.</param>
        /// <param name="treeOperation">The tree opeartion.</param>
        public CommitOperation(IDataProvider dataProvider, ITreeOperation treeOperation)
            : this(dataProvider, treeOperation, NullLogger<CommitOperation>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data Provider.</param>
        /// <param name="treeOperation">The tree operation.</param>
        /// <param name="logger">The Logger factory.</param>
        public CommitOperation(
            IDataProvider dataProvider,
            ITreeOperation treeOperation,
            ILogger<CommitOperation> logger)
        {
            this.dataProvider = dataProvider;
            this.treeOperation = treeOperation;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public string CreateCommit(string message)
        {
            this.CommitValidate();
            string commit = $"tree {this.treeOperation.WriteTree()}\n";
            string HEAD = this.dataProvider.GetRef(Constants.HEAD).Value;
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            string mergeHead = this.dataProvider.GetRef(Constants.MergeHEAD).Value;
            if (!string.IsNullOrWhiteSpace(mergeHead))
            {
                commit += $"parent {mergeHead}\n";
                this.dataProvider.DeleteRef(Constants.MergeHEAD, false);
            }

            if (this.dataProvider.Config != null && this.dataProvider.Config.Author != null)
            {
                commit += $"author {this.dataProvider.Config.Author}\n";
            }
            else
            {
                commit += $"author unknown\n";
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = this.dataProvider.WriteObject(commit.Encode(), Constants.Commit);
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
            return oid;
        }

        /// <inheritdoc/>
        public Commit GetCommit(string oid)
        {
            List<string> parents = new ();
            var commit = this.dataProvider.GetObject(oid, Constants.Commit).Decode();
            string[] lines = commit.Split("\n");
            string tree = null;
            int index;
            Author author = null;
            for (index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                string[] tokens = line.Split(new char[] { ' ', ':' });
                if (tokens[0].Equals(Constants.Tree))
                {
                    tree = tokens[1];
                }

                if (tokens[0].Equals(Constants.Parent))
                {
                    parents.Add(tokens[1]);
                }

                if (tokens[0].Equals(Constants.Author))
                {
                    Author.TryParse(string.Join(' ', tokens.TakeLast(tokens.Length - 1)), out author);
                }
            }

            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parents = parents,
                Message = message,
                Author = author,
            };
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetCommitHistory(IEnumerable<string> oids)
        {
            Deque<string> oidQueue = new Deque<string>(oids);
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            while (oidQueue.Count > 0)
            {
                string oid = oidQueue.RemoveFromFront();
                if (string.IsNullOrWhiteSpace(oid) || visited.Contains(oid))
                {
                    continue;
                }

                visited.Add(oid);
                yield return oid;

                var commit = this.GetCommit(oid);
                oidQueue.AddToFront(commit.Parents.FirstOrDefault());
                if (commit.Parents.Count > 1)
                {
                    commit.Parents.TakeLast(commit.Parents.Count - 1)
                        .ToList()
                        .ForEach(id => oidQueue.AddToBack(id));
                }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetObjectHistory(IEnumerable<string> oids)
        {
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            IEnumerable<string> IterObjectInTree(string oid)
            {
                visited.Add(oid);
                yield return oid;

                foreach (var (type, subOid, _) in this.treeOperation.IterTreeEntry(oid))
                {
                    if (type == Constants.Tree)
                    {
                        foreach (var val in IterObjectInTree(subOid))
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

            foreach (var oid in this.GetCommitHistory(oids))
            {
                yield return oid;
                var commit = this.GetCommit(oid);
                if (!visited.Contains(commit.Tree))
                {
                    foreach (var val in IterObjectInTree(commit.Tree))
                    {
                        yield return val;
                    }
                }
            }
        }

        private void CommitValidate()
        {
            string HEAD = this.dataProvider.GetRef(Constants.HEAD).Value;
            Tree headTree = new Tree();
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                Commit commit = this.GetCommit(HEAD);
                headTree = this.treeOperation.GetTree(commit.Tree);
            }

            Tree indexTree = this.treeOperation.GetIndexTree();
            bool isSame = true;

            if (headTree.Count != indexTree.Count)
            {
                isSame = false;
            }
            else
            {
                foreach (var headTreeEntry in headTree)
                {
                    if (!indexTree.ContainsKey(headTreeEntry.Key))
                    {
                        isSame = false;
                        break;
                    }

                    if (indexTree[headTreeEntry.Key] != headTree[headTreeEntry.Key])
                    {
                        isSame = false;
                        break;
                    }
                }
            }

            if (isSame)
            {
                this.logger.LogError("nothing to commit (create/copy files and use \"ugit add\" to track.");
                throw new UgitException("nothing to commit (create/copy files and use \"ugit add\" to track.");
            }
        }
    }
}
