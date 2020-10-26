namespace Ugit.Options
{
    using CommandLine;

    [Verb("merge-base")]
    internal class MergeBaseOption
    {
        [Value(0)]
        public string Commit1 { get; set; }

        [Value(1)]
        public string Commit2 { get; set; }
    }
}
