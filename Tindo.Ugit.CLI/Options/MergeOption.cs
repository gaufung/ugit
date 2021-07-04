namespace Ugit.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Merge options.
    /// </summary>
    [Verb("merge")]
    [ExcludeFromCodeCoverage]
    internal class MergeOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}
