using CommandLine;

namespace Tindo.UgitCLI.Options
{
    [Verb("set-url")]
    public class RemoteUrlOption
    {
        [Value(0, Required = true)]
        public string Name { get; set; }

        [Value(1, Required = true)]
        public string Url { get; set; }
    }
}