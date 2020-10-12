using CommandLine;

namespace Ugit.Options
{
    [Verb("commit", HelpText ="Make a commit")]
    internal class CommitOption
    {
        [Value(0)]
        [Option(shortName:'m', longName:"message")]
        public string Message { get; set; }
    }
}
