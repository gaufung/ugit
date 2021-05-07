﻿namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    /// <summary>
    /// The default implementation of <see cref="IDiffOperation"/>.
    /// </summary>
    public class DefaultDiffOperation : IDiffOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly IDiffProxyOperation diffProxy;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultDiffOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">the data provider.</param>
        /// <param name="diffProxy">the diff command proxy.</param>
        public DefaultDiffOperation(IDataProvider dataProvider, IDiffProxyOperation diffProxy)
        {
            this.dataProvider = dataProvider;
            this.diffProxy = diffProxy;
        }

        /// <inheritdoc/>
        public IEnumerable<(string, IEnumerable<string>)> CompareTrees(params Tree[] trees)
        {
#if NET5_0
            Dictionary<string, string[]> entries = new ();
#else
            Dictionary<string, string[]> entries = new Dictionary<string, string[]>();
#endif
            for (int i = 0; i < trees.Length; i++)
            {
                Tree tree = trees[i];
                foreach (var entry in tree)
                {
                    string path = entry.Key;
                    string oid = entry.Value;
                    if (!entries.ContainsKey(path))
                    {
                        entries[path] = new string[trees.Length];
                    }

                    entries[path][i] = oid;
                }
            }

            foreach (var entry in entries)
            {
                yield return (entry.Key, entry.Value);
            }
        }

        /// <inheritdoc/>
        public string DiffBlob(string fromOid, string toOid, string path)
        {
            string fromFile = Path.GetTempFileName();
            this.dataProvider.Write(fromFile, this.dataProvider.GetObject(fromOid));
            string toFile = Path.GetTempFileName();
            this.dataProvider.Write(toFile, this.dataProvider.GetObject(toOid));
            var (_, output, _) = this.diffProxy.Execute(
                "diff",
                $"--unified --show-c-function --label a/{path} {fromFile} --label b/{path} {toFile}");
            return output;
        }

        /// <inheritdoc/>
        public string DiffTrees(Tree fromTree, Tree toTree)
        {
            return string.Join(
                "\n",
                this.CompareTrees(fromTree, toTree)
                .Where(t => t.Item2.First() != t.Item2.Last())
                .Select(t => this.DiffBlob(t.Item2.First(), t.Item2.Last(), t.Item1)));
        }

        /// <inheritdoc/>
        public IEnumerable<(string, string)> IterChangedFiles(Tree fromTree, Tree toTree)
        {
            foreach (var entry in this.CompareTrees(fromTree, toTree))
            {
                string path = entry.Item1;
                string fromOid = entry.Item2.First();
                string toOid = entry.Item2.Last();
                if (fromOid != toOid)
                {
                    string action;
                    if (string.IsNullOrEmpty(fromOid))
                    {
                        action = "new file";
                    }
                    else if (string.IsNullOrEmpty(toOid))
                    {
                        action = "deleted";
                    }
                    else
                    {
                        action = "modified";
                    }

                    yield return (path, action);
                }
            }
        }

        /// <inheritdoc/>
        public string MergeBlob(string headOid, string otherOid)
        {
            string headFile = Path.GetTempFileName();
            this.dataProvider.Write(headFile, this.dataProvider.GetObject(headOid));
            string otherFile = Path.GetTempFileName();
            this.dataProvider.Write(otherFile, this.dataProvider.GetObject(otherOid));
            string arguments = string.Join(" ", new string[] { "-DHEAD", headFile, otherFile });
            var (_, output, _) = this.diffProxy.Execute("diff", arguments);
            return output;
        }

        /// <inheritdoc/>
        public Tree MergeTree(Tree headTree, Tree otherTree)
        {
#if NET5_0
            Tree tree = new ();
#else
            Dictionary<string, string> tree = new Dictionary<string, string>();
#endif
            foreach (var entry in this.CompareTrees(headTree, otherTree))
            {
                string path = entry.Item1;
                string headOid = entry.Item2.First();
                string otherOid = entry.Item2.Last();
                tree[path] = this.dataProvider.HashObject(this.MergeBlob(headOid, otherOid).Encode());
            }

            return tree;
        }
    }
}