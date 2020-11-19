using CommandLine;

namespace Ugit.Options
{
    [Verb("fetch")]
    internal class FetchOption
    {
        [Value(0)]
        public string Remote { get; set; }
    }
}
