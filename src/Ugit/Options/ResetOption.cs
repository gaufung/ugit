namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Reset option
    /// </summary>
    [Verb("reset")]
    public class ResetOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}
