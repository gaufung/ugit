using CommandLine;

namespace Ugit.Options
{
    [Verb("log")]
    internal class LogOption
    {
        [Value(0)]
        public string Oid { get; set; }
    }
}
