using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Nito.Collections;

namespace Tindo.UgitCore.Operations
{
    public class CommitOperation : ICommitOperation
    {
        private readonly IDataOperator dataOperator;

        private readonly ITreeOperation treeOperation;

        private readonly ILogger<CommitOperation> logger;

        public CommitOperation(IDataOperator dataOperator, ITreeOperation treeOperation, ILoggerFactory loggerFactory)
        {
            this.dataOperator = dataOperator;
            this.treeOperation = treeOperation;
            this.logger = loggerFactory.CreateLogger<CommitOperation>();
        }

        public string Create(string message)
        {
            this.Validate();
            string commit = $"tree {this.treeOperation.Write()}\n";
            string HEAD = this.dataOperator.GetRef(Constants.HEAD).Value;
            if (!HEAD.IsNullOrWhiteSpace())
            {
                commit += $"parent {HEAD}\n";
            }

            string mergeHead = this.dataOperator.GetRef(Constants.MergeHEAD).Value;
            if (!mergeHead.IsNullOrWhiteSpace())
            {
                commit += $"parent {mergeHead}\n";
                this.dataOperator.DeleteRef(Constants.MergeHEAD, false);
            }

            if (dataOperator.Config.Author.HasValue)
            {
                commit += $"{Constants.Author}{Constants.Whitespace}{dataOperator.Config.Author.Value}\n";
            }
            else
            {
                commit += $"{Constants.Author}{Constants.Whitespace}{Author.DefaultAuthor}\n";
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = this.dataOperator.WriteObject(commit.Encode(), Constants.Commit);
            this.dataOperator.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
            return oid;

        }

        public Commit Get(string oid)
        {
            List<string> parents = new List<string>();
            var commit = this.dataOperator.GetObject(oid, Constants.Commit).Decode();
            string[] lines = commit.Split('\n');
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
                    Author.Parse(string.Join("", tokens.TakeLast(tokens.Length - 1)), ref author);
                }
            }

            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parents = parents,
                Message = message,
                Author = author
            };
        }

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

        public IEnumerable<string> GetObjectsFromHistory(IEnumerable<string> oids)
        {
            HashSet<string> visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            IEnumerable<string> IterObjectInTree(string oid)
            {
                visited.Add(oid);
                yield return oid;

                foreach (var (type, subOid, _) in this.treeOperation.Iterate(oid))
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

        private void Validate()
        {
            string HEAD = this.dataOperator.GetRef(Constants.HEAD).Value;
            Tree headTree = new();
            if (!HEAD.IsNullOrWhiteSpace())
            {
                Commit commit = this.Get(HEAD);
                headTree = this.treeOperation.Get(commit.Tree);
            }

            Tree indextree = this.treeOperation.IndexTree;

            bool isSame = true;

            if (headTree.Count != indextree.Count)
            {
                isSame = false;
            }
            else
            {
                foreach (var headTreeEntry in headTree)
                {
                    if (!indextree.ContainsKey(headTreeEntry.Key))
                    {
                        isSame = false;
                        break;
                    }
                    if (indextree[headTreeEntry.Key] == headTreeEntry.Value)
                    {
                        continue;
                    }

                    isSame = false;
                    break;
                }
            }

            if (isSame)
            {
                throw new UgitException("Nothing to commit (create/copy files and use \"ugit add\" to track)");
            }
        }
    }
}
