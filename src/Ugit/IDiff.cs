namespace Ugit
{
    using System.Collections.Generic;

    internal interface IDiff
    {
        string DiffTree(IDictionary<string, string> fromTree, IDictionary<string, string> toTree);

        IEnumerable<(string, IEnumerable<string>)> CompareTrees(params IDictionary<string, string>[] trees);

        string DiffBlob(string fromOid, string toOid, string path);

        IEnumerable<(string, string)> IterChangedFiles(IDictionary<string, string> fromTree, IDictionary<string, string> toTree);

        IDictionary<string, string> MergeTree(IDictionary<string, string> headTree, IDictionary<string, string> otherTree);

        string MergeBlob(string headOid, string otherOid);
    }
}
