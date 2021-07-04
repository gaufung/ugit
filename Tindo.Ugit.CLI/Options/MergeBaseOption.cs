namespace Ugit.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Merge Base option
    /// </summary>
    [Verb("merge-base")]
    [ExcludeFromCodeCoverage]
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
