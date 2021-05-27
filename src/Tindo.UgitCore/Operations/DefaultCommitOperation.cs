namespace Tindo.UgitCore.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Nito.Collections;
    using Microsoft.Extensions.Logging;

    public class DefaultCommitOperation : ICommitOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        private readonly ILogger<DefaultCommitOperation> logger; 

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data Provider.</param>
        /// <param name="treeOperation">The tree operation.</param>
        public DefaultCommitOperation(IDataProvider dataProvider, ITreeOperation treeOperation,
            ILoggerFactory loggerFactory)
        {
            this.dataProvider = dataProvider;
            this.treeOperation = treeOperation;
            this.logger = loggerFactory.CreateLogger<DefaultCommitOperation>();
        }

        /// <inheritdoc/>
        public string Create(string message)
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
            
            if (dataProvider.Config.Author.HasValue)
            {
                commit += $"{Constants.Author}{Constants.Whitespace}{dataProvider.Config.Author.Value}\n";
            }
            else
            {
                commit += $"{Constants.Author}{Constants.Whitespace}{Author.DefaultAuthor}\n";
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = this.dataProvider.HashObject(commit.Encode(), Constants.Commit);
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
            return oid;
        }

        /// <inheritdoc/>
        public Commit Get(string oid)
        {
            List<string> parents = new ();
            var commit = this.dataProvider.GetObject(oid, Constants.Commit).Decode();
            string[] lines = commit.Split("\n");
            string tree = null;
            int index;
            Author author = default;
            for (index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                string[] tokens = line.Split(' ');
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
                    Author.Parse(string.Join("", tokens.TakeLast(tokens.Length-1)), ref author);
                }
            }
            
            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parents = parents,
                Message = message,
                Author =  author
            };
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetHistory(IEnumerable<string> oids)
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

                var commit = this.Get(oid);
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

            foreach (var oid in this.GetHistory(oids))
            {
                yield return oid;
                var commit = this.Get(oid);
                if (!visited.Contains(commit.Tree))
                {
                    foreach (var val in IterObjectInTree(commit.Tree))
                    {
                        yield return val;
                    }
                }
            }
        }

        // Compare index and repo tree.
        private void CommitValidate()
        {
            string HEAD = this.dataProvider.GetRef(Constants.HEAD).Value;
            Tree headTree = new Tree();
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                Commit commit = this.Get(HEAD);
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

                    if (indexTree[headTreeEntry.Key] == headTree[headTreeEntry.Key])
                    {
                        continue;
                    }

                    isSame = false;
                    break;
                }
            }

            if (isSame)
            {
                throw new UgitException("nothing to commit (create/copy files and use \"ugit add\" to track.");
            }
        }
    }
}