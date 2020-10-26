namespace Ugit.Options
{
    using CommandLine;

    [Verb("checkout")]
    internal class CheckoutOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
