using CommandLine;

namespace Ugit.Options
{
    [Verb("cat-file", HelpText="Display the object by object id.")]
    internal class CatFileOption
    {
        [Value(0)]
        public string Object { get; set; }
    }
}
