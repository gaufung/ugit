namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Reset option
    /// </summary>
    [Verb("reset")]
    [ExcludeFromCodeCoverage]
    public class ResetOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}