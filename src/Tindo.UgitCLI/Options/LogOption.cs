namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Log option
    /// </summary>
    [Verb("log")]
    [ExcludeFromCodeCoverage]
    internal class LogOption
    {
        /// <summary>
        /// Gets or sets th oid.
        /// </summary>
        [Value(0, Default = "@")]
        public string Oid { get; set; }
    }
}