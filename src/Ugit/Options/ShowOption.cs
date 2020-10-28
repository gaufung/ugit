namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Show option.
    /// </summary>
    [Verb("show")]
    internal class ShowOption
    {
        /// <summary>
        /// Gets or sets oid.
        /// </summary>
        [Value(0, Default ="@")]
        public string Oid { get; set; }
    }
}
