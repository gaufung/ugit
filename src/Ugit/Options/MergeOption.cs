namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Merge options.
    /// </summary>
    [Verb("merge")]
    internal class MergeOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}
