using System;
using System.Collections.Generic;

namespace ugit
{
    public class Diff
    {
        private static IDictionary<string, string[]> CompareTrees(params IDictionary<string, string>[] trees)
        {
            IDictionary<string, string[]> output = new Dictionary<string, string[]>();
            for (int i = 0; i < trees.Length; i++)
            {
                var tree = trees[i];
                foreach (var entry in tree)
                {
                    string path = entry.Key;
                    string oid = entry.Value;
                    if (!output.ContainsKey(path))
                    {
                        output[path] = new string[trees.Length];
                    }

                    output[path][i] = oid;
                }
            }

            return output;
        }

        public static string DiffTree(IDictionary<string, string> @from, IDictionary<string, string> to)
        {
            string output = "";
            foreach (var entry in CompareTrees(@from, to))
            {
                string path = entry.Key;
                string fromObject = entry.Value[0];
                string toObject = entry.Value[1];
                if (fromObject != toObject)
                {
                    output = $"changed: {path}\n";
                }
            }

            return output;
        }

        public static IEnumerable<ValueTuple<string, string>> IterChangedFiles(IDictionary<string, string> @from, IDictionary<string, string> to)
        {
            foreach (var entry in CompareTrees(@from, to))
            {
                string path = entry.Key;
                string fromObject = entry.Value[0];
                string toObject = entry.Value[1];
                if (fromObject != toObject)
                {
                    string action;
                    if (string.IsNullOrWhiteSpace(fromObject))
                    {
                        action = "new file";
                    }
                    else if (string.IsNullOrWhiteSpace(toObject))
                    {
                        action = "deleted";
                    }
                    else
                    {
                        action = "modified";
                    }

                    yield return ValueTuple.Create(path, action);
                }
            }
        }
        
        
    }
}