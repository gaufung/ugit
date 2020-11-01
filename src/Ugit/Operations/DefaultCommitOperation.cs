namespace Ugit.Operations
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default implmentation of <see cref="ICommitOperation"/>.
    /// </summary>
    internal class DefaultCommitOperation : ICommitOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ITreeOperation treeOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultCommitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data Provider.</param>
        /// <param name="treeOperation">The tree opeartion.</param>
        public DefaultCommitOperation(IDataProvider dataProvider, ITreeOperation treeOperation)
        {
            this.dataProvider = dataProvider;
            this.treeOperation = treeOperation;
        }

        /// <inheritdoc/>
        public string CreateCommit(string message)
        {
            string commit = $"tree {this.treeOperation.WriteTree()}\n";
            string HEAD = this.dataProvider.GetRef("HEAD").Value;
            if (!string.IsNullOrWhiteSpace(HEAD))
            {
                commit += $"parent {HEAD}\n";
            }

            string mergeHead = this.dataProvider.GetRef("MERGE_HEAD").Value;
            if (!string.IsNullOrWhiteSpace(mergeHead))
            {
                commit += $"parent {mergeHead}\n";
                this.dataProvider.DeleteRef("MERGE_HEAD", false);
            }

            commit += "\n";
            commit += $"{message}\n";
            string oid = this.dataProvider.HashObject(commit.Encode(), "commit");
            this.dataProvider.UpdateRef("HEAD", RefValue.Create(false, oid));
            return oid;
        }

        /// <inheritdoc/>
        public Commit GetCommit(string oid)
        {
            var parents = new List<string>();
            var commit = this.dataProvider.GetObject(oid, "commit").Decode();
            string[] lines = commit.Split("\n");
            string tree = null;
            int index;
            for (index = 0; index < lines.Length; index++)
            {
                string line = lines[index];
                if (string.IsNullOrWhiteSpace(line))
                {
                    break;
                }

                string[] tokens = line.Split(' ');
                if (tokens[0].Equals("tree"))
                {
                    tree = tokens[1];
                }

                if (tokens[0].Equals("parent"))
                {
                    parents.Add(tokens[1]);
                }
            }

            string message = string.Join("\n", lines.TakeLast(lines.Length - index - 1));
            return new Commit
            {
                Tree = tree,
                Parents = parents,
                Message = message,
            };
        }
    }
}
