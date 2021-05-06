namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Fetch option
    /// </summary>
    [Verb("fetch")]
    [ExcludeFromCodeCoverage]
    internal class FetchOption
    {
        /// <summary>
        /// Gets or sets the remote.
        /// </summary>
        [Value(0)]
        public string Remote { get; set; }
    }
}