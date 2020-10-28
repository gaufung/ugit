namespace Ugit.Options
{
    using CommandLine;

    [Verb("commit", HelpText ="Make a commit")]
    internal class CommitOption
    {
        [Value(0)]
        [Option(shortName: 'm', longName: "message")]
        public string Message { get; set; }
    }
}
