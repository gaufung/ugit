using CommandLine;

namespace Ugit.Options
{
    [Verb("merge")]
    internal class MergeOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
