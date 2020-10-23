using CommandLine;

namespace Ugit.Options
{
    [Verb("reset")]
    public class ResetOption
    {
        [Value(0)]
        public string Commit { get; set; }
    }
}
