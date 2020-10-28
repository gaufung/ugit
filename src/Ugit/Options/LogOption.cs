namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Log option
    /// </summary>
    [Verb("log")]
    internal class LogOption
    {
        /// <summary>
        /// Gets or sets th oid.
        /// </summary>
        [Value(0, Default = "@")]
        public string Oid { get; set; }
    }
}
