using CommandLine;

namespace Ugit.Options
{
    [Verb("tag")]
    internal class TagOption
    {
        [Value(0)]
        public string Name { get; set; }

        [Value(1, Required = false)]
        public string Oid { get; set; }
    }
}
