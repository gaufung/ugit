using CommandLine;
using System.Collections.Generic;

namespace Ugit.Options
{
    [Verb("add")]
    internal class AddOption
    {
        [Value(0)]
        public IEnumerable<string> Files { get; set; }
    }
}
