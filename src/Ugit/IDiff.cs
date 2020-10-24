using System.Collections.Generic;

namespace Ugit
{
    internal interface IDiff
    {
        string DiffTree(IDictionary<string, string> fromTree, IDictionary<string, string> toTree);

        IEnumerable<(string, IEnumerable<string>)> CompareTrees(params IDictionary<string, string>[] trees);
    }
}
