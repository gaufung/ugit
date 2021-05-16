using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace Tindo.UgitCLI.Options
{
    [Verb("config")]
    [ExcludeFromCodeCoverage]
    internal class ConfigOption
    {
        [Value(0)]
        [Option(longName:"user.name")]
        public string Name { get; set; }
        
        [Value(1)]
        [Option(longName: "user.email")]
        public string Email { get; set; }
    }
}