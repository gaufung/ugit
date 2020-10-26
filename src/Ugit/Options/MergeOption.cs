namespace Ugit.Options
{
    using CommandLine;

    [Verb("merge")]
    internal class MergeOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
