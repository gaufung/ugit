namespace Ugit.Options
{
    using CommandLine;

    [Verb("read-tree", HelpText="Read directory from tree object Id.")]
    internal class ReadTreeOption
    {
        [Value(0)]
        public string Tree { get; set; }
    }
}
