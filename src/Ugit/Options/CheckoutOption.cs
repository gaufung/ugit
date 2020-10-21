using CommandLine;

namespace Ugit.Options
{

    [Verb("checkout")]
    internal class CheckoutOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
