namespace Ugit.Options
{
    using CommandLine;

    [Verb("log")]
    internal class LogOption
    {
        [Value(0, Default = "@")]
        public string Oid { get; set; }
    }
}
