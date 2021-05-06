namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Commit option.
    /// </summary>
    [Verb("checkout")]
    [ExcludeFromCodeCoverage]
    internal class CheckoutOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}