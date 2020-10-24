using System.Collections.Generic;

namespace Ugit
{
    internal interface IDiff
    {
        string DiffTree(IDictionary<string, string> fromTree, IDictionary<string, string> toTree);

        IEnumerable<(string, IEnumerable<string>)> CompareTrees(params IDictionary<string, string>[] trees);

        string DiffBlob(string fromOid, string toOid, string path);

        IEnumerable<(string, string)> IterChangedFiles(IDictionary<string, string> fromTree, IDictionary<string, string> toTree);
    }
}
