namespace Ugit.Options
{
    using CommandLine;

    [Verb("diff")]
    internal class DiffOption
    {
        [Value(0, Default ="@")]
        public string Commit { get; set; }
    }
}
