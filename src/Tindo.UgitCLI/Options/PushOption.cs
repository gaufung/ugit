namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// push option
    /// </summary>
    [Verb("push")]
    [ExcludeFromCodeCoverage]
    internal class PushOption
    {
        /// <summary>
        /// Gets or sets the remote.
        /// </summary>
        [Value(0)]
        public string Remote { get; set; }

        /// <summary>
        /// Gets or sets the branch
        /// </summary>
        [Value(1)]
        public string Branch { get; set; }
    }
}