using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Ugit
{
    internal class Diff : IDiff
    {
        public IEnumerable<(string, IEnumerable<string>)> CompareTrees(params IDictionary<string, string>[] trees)
        {
            IDictionary<string, string[]> entries = new Dictionary<string, string[]>();
            for(int i = 0; i < trees.Length; i++)
            {
                IDictionary<string, string> tree = trees[i];
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

        public string DiffTree(IDictionary<string, string> fromTree, IDictionary<string, string> toTree)
        {
            return string.Join(
                "\n", 
                CompareTrees(fromTree, toTree)
                .Where(t => t.Item2.First() != t.Item2.Last())
                .Select(t => $"changed: {t.Item1}")
                );
        }
    }
}
