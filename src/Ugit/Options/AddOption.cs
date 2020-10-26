namespace Ugit.Options
{
    using System.Collections.Generic;
    using CommandLine;

    [Verb("add")]
    internal class AddOption
    {
        [Value(0)]
        public IEnumerable<string> Files { get; set; }
    }
}
