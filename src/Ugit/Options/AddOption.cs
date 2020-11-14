namespace Ugit.Options
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Add Option.
    /// </summary>
    [Verb("add")]
    [ExcludeFromCodeCoverage]
    internal class AddOption
    {
        /// <summary>
        /// Gets or sets the files.
        /// </summary>
        [Value(0)]
        public IEnumerable<string> Files { get; set; }
    }
}
