namespace Tindo.UgitCore.Operations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Default implementation of <see cref="IMergeOperation"/>.
    /// </summary>
    public class DefaultMergeOperation : IMergeOperation
    {
        private readonly ICommitOperation commitOperation;
        private readonly IDataProvider dataProvider;
        private readonly IDiffOperation diff;
        private readonly ITreeOperation treeOperation;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultMergeOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="commitOperation">The commit operation.</param>
        /// <param name="treeOperation">The tree operation.</param>
        /// <param name="diff">The diff operation.</param>
        public DefaultMergeOperation(
            IDataProvider dataProvider,
            ICommitOperation commitOperation,
            ITreeOperation treeOperation,
            IDiffOperation diff)
        {
            this.dataProvider = dataProvider;
            this.commitOperation = commitOperation;
            this.treeOperation = treeOperation;
            this.diff = diff;
        }

        /// <inheritdoc/>
        public void Merge(string other)
        {
            string head = this.dataProvider.GetRef("HEAD").Value;
            var headCommit = this.commitOperation.Get(head);
            string mergeBase = this.GetMergeBase(other, head);
            var otherCommit = this.commitOperation.Get(other);

            if (mergeBase == head)
            {
                this.treeOperation.ReadTree(otherCommit.Tree, true);
                this.dataProvider.UpdateRef("HEAD", RefValue.Create(false, other));
                Console.WriteLine("Fast-forward, no need to commit");
                return;
            }

            this.dataProvider.UpdateRef("MERGE_HEAD", RefValue.Create(false, other));
            this.ReadTreeMerged(headCommit.Tree, otherCommit.Tree, true);
            Console.WriteLine("Merged in working tree\nPlease commit");
        }

        private string GetMergeBase(string oid1, string oid2)
        {
            IEnumerable<string> parents = this.commitOperation.GetHistory(new[] { oid1 });
            foreach (var oid in this.commitOperation.GetHistory(new[] { oid2 }))
            {
                if (parents.Contains(oid))
                {
                    return oid;
                }
            }

            return null;
        }

        private void ReadTreeMerged(string headTree, string otherTree, bool updateWorking = false)
        {
            var index = this.dataProvider.Index;
            index.Clear();
            index.Update(this.diff.MergeTree(this.treeOperation.GetTree(headTree), this.treeOperation.GetTree(otherTree)));
            if (updateWorking)
            {
                this.treeOperation.CheckoutIndex(index);
            }

            this.dataProvider.Index = index;
        }
    }
}