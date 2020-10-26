namespace Ugit.Options
{
    using CommandLine;

    [Verb("branch")]
    internal class BranchOption
    {
        [Value(0, Required = false)]
        public string Name { get; set; }

        [Value(1, Required = false, Default = "@")]
        public string StartPoint { get; set; }
    }
}
