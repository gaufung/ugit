using CommandLine;
using System;
using System.IO.Abstractions;
using Ugit.Options;

namespace Ugit
{
    class Program
    {
        static readonly IDataProvider dataProvider;

        static readonly IFileSystem fileSystem;

        static Program()
        {
            fileSystem = new FileSystem();
            dataProvider = new DataProvider();
        }

        static int Main(string[] args)
        {
            int exitCode = Parser.Default.ParseArguments<
                InitOption,
                HashObjectOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                errors => 1);
            return exitCode;
        }

        static int Init(InitOption o)
        {
            dataProvider.Init();
            Console.WriteLine($"Initialized empty ugit repository in {dataProvider.GitDirFullPath}");
            return 0;
        }

        static int HashObject(HashObjectOption o)
        {
            byte[] data = fileSystem.File.ReadAllBytes(o.File);
            Console.WriteLine(dataProvider.HashObject(data));
            return 0;
        }
    }
}
