namespace Ugit.Options
{
    using CommandLine;

    /// <summary>
    /// Commit option.
    /// </summary>
    [Verb("checkout")]
    internal class CheckoutOption
    {
        /// <summary>
        /// Gets or sets the commit.
        /// </summary>
        [Value(0)]
        public string Commit { get; set; }
    }
}
