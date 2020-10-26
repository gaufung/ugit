namespace Ugit.Options
{
    using CommandLine;

    [Verb("reset")]
    public class ResetOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
