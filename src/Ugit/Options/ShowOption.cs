namespace Ugit.Options
{
    using CommandLine;

    [Verb("show")]
    internal class ShowOption
    {
        [Value(0, Default ="@")]
        public string Oid { get; set; }
    }
}
