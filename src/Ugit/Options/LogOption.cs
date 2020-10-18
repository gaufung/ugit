using CommandLine;

namespace Ugit.Options
{
    [Verb("log")]
    internal class LogOption
    {
        [Value(0, Default = "@")]
        public string Oid { get; set; }
    }
}
