namespace Tindo.Ugit.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// Diff interface.
    /// </summary>
    internal interface IDiffOperation
    {
        /// <summary>
        /// Diff a tree.
        /// </summary>
        /// <param name="fromTree">The first tree respents the snapshot. {filePath:oid}.</param>
        /// <param name="toTree">The second tree respents the snapshot. {filePath:oid}.</param>
        /// <returns>The difference.</returns>
        string DiffTrees(Tree fromTree, Tree toTree);

        /// <summary>
        /// Compare multiple tree.
        /// </summary>
        /// <param name="trees">The trees to compare.</param>
        /// <returns>The difference.{filepath, [oids]}.</returns>
        IEnumerable<(string, IEnumerable<string>)> CompareTrees(params Tree[] trees);

        /// <summary>
        /// The differnce of two blob files.
        /// </summary>
        /// <param name="fromOid">The first file blob oid.</param>
        /// <param name="toOid">The second file blob oid.</param>
        /// <param name="path">The file path.</param>
        /// <returns>The diff result.</returns>
        string DiffBlob(string fromOid, string toOid, string path);

        /// <summary>
        /// The two trees comparsion.
        /// </summary>
        /// <param name="fromTree">The first tree.</param>
        /// <param name="toTree">The second tree.</param>
        /// <returns>The differnce specification. {filepath: action}</returns>
        IEnumerable<(string, string)> IterChangedFiles(Tree fromTree, Tree toTree);

        /// <summary>
        /// Merge two tree.
        /// </summary>
        /// <param name="headTree">The first tree.</param>
        /// <param name="otherTree">The second tree.</param>
        /// <returns>The Merge result.</returns>
        Tree MergeTree(Tree headTree, Tree otherTree);

        /// <summary>
        /// Merge blob files.
        /// </summary>
        /// <param name="headOid">The first file oid.</param>
        /// <param name="otherOid">The second file oid.</param>
        /// <returns>The merge result.</returns>
        string MergeBlob(string headOid, string otherOid);
    }
}
