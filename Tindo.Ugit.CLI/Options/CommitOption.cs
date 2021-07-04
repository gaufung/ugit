namespace Ugit.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    [Verb("commit", HelpText ="Make a commit")]
    [ExcludeFromCodeCoverage]
    internal class CommitOption
    {
        [Value(0)]
        [Option(shortName: 'm', longName: "message")]
        public string Message { get; set; }
    }
}
