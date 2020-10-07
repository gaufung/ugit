using CommandLine;
using System;
using Ugit.Options;

namespace Ugit
{
    class Program
    {
        static readonly IDataProvider dataProvider;

        static Program()
        {
            dataProvider = new DataProvider();
        }

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
            dataProvider.Init();
            Console.WriteLine($"Initialized empty ugit repository in {dataProvider.GitDirFullPath}");
            return 0;
        }
    }
}
