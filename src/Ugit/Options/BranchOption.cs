using CommandLine;

namespace Ugit.Options
{
    [Verb("branch")]
    internal class BranchOption
    {
        [Value(0, Required = true)]
        public string Name { get; set; }

        [Value(1, Required = false, Default = "@")]
        public string StartPoint { get; set; }
    }
}
