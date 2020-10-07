using CommandLine;
using System;
using Ugit.Options;

namespace Ugit
{
    class Program
    {
        static int Main(string[] args)
        {
            int exitCode = Parser.Default.ParseArguments<
                InitOption>(args).MapResult(
                (InitOption o) => Init(o),
                errors => 1);
            return exitCode;
        }

        static int Init(InitOption o)
        {
            Console.WriteLine("Hello ugit!");
            return 0;
        }
    }
}
