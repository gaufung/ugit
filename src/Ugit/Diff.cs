using DiffPlex.DiffBuilder;
using System.Collections.Generic;
using System.Linq;

namespace Ugit
{
    internal class Diff : IDiff
    {
        private readonly IDataProvider dataProvider;

        public Diff(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

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

        public string DiffBlob(string fromOid, string toOid, string path)
        {
            string fromText = dataProvider.GetObject(fromOid).Decode();
            string toText = dataProvider.GetObject(toOid).Decode();
            var model = InlineDiffBuilder.Diff(fromText, toText);
            return model.Show(path);
        }

        public string DiffTree(IDictionary<string, string> fromTree, IDictionary<string, string> toTree)
        {
            return string.Join(
                "\n",
                CompareTrees(fromTree, toTree)
                .Where(t => t.Item2.First() != t.Item2.Last())
                .Select(t => DiffBlob(t.Item2.First(), t.Item2.Last(), t.Item1))
                );
        }

        public IEnumerable<(string, string)> IterChangedFiles(IDictionary<string, string> fromTree, IDictionary<string, string> toTree)
        {
            foreach(var entry in CompareTrees(fromTree, toTree))
            {
                string path = entry.Item1;
                string fromOid = entry.Item2.First();
                string toOid = entry.Item2.Last();
                if(fromOid!=toOid)
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
    }
}
