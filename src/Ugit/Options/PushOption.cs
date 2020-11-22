namespace Ugit.Options
{
    using CommandLine;

    [Verb("push")]
    internal class PushOption
    {
        [Value(0)]
        public string Remote { get; set; }

        [Value(1)]
        public string Branch { get; set; }
    }
}
