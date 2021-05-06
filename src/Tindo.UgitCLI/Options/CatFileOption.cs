namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Cat file option.
    /// </summary>
    [Verb("cat-file", HelpText="Display the object by object id.")]
    [ExcludeFromCodeCoverage]
    internal class CatFileOption
    {
        /// <summary>
        /// Gets or sets the Object.
        /// </summary>
        [Value(0)]
        public string Object { get; set; }
    }
}