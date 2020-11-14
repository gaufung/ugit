namespace Ugit.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Show option.
    /// </summary>
    [Verb("show")]
    [ExcludeFromCodeCoverage]
    internal class ShowOption
    {
        /// <summary>
        /// Gets or sets oid.
        /// </summary>
        [Value(0, Default ="@")]
        public string Oid { get; set; }
    }
}
