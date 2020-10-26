namespace Ugit.Options
{
    using CommandLine;

    [Verb("cat-file", HelpText="Display the object by object id.")]
    internal class CatFileOption
    {
        [Value(0)]
        public string Object { get; set; }
    }
}
