namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Diff Option.
    /// </summary>
    [Verb("diff")]
    internal class DiffOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0, Default ="@")]
        public string Commit { get; set; }
    }
}
