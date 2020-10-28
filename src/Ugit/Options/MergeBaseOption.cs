namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Merge Base option
    /// </summary>
    [Verb("merge-base")]
    internal class MergeBaseOption
    {
        /// <summary>
        /// Gets or sets commit one.
        /// </summary>
        [Value(0)]
        public string Commit1 { get; set; }

        /// <summary>
        /// Gets or sets commit two.
        /// </summary>
        [Value(1)]
        public string Commit2 { get; set; }
    }
}
