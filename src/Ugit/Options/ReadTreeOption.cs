namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Read tree option.
    /// </summary>
    [Verb("read-tree", HelpText="Read directory from tree object Id.")]
    internal class ReadTreeOption
    {
        /// <summary>
        /// Gets or sets tree.
        /// </summary>
        [Value(0)]
        public string Tree { get; set; }
    }
}
