namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Cat file option.
    /// </summary>
    [Verb("cat-file", HelpText="Display the object by object id.")]
    internal class CatFileOption
    {
        /// <summary>
        /// Gets or sets the Object.
        /// </summary>
        [Value(0)]
        public string Object { get; set; }
    }
}
