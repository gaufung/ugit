using CommandLine;

namespace Ugit.Options
{
    [Verb("diff")]
    internal class DiffOption
    {
        [Value(0, Default ="@")]
        public string Commit { get; set; }
    }
}
