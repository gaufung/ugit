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

        static readonly IBaseOperator baseOperator;

        static readonly Func<string, string> OidConverter;

        static Program()
        {
            fileSystem = new FileSystem();
            dataProvider = new DataProvider();
            baseOperator = new BaseOperator(fileSystem, dataProvider);
            OidConverter = baseOperator.GetOid;
        }

        static int Main(string[] args)
        {
            int exitCode = Parser.Default.ParseArguments<
                InitOption,
                HashObjectOption,
                CatFileOption,
                WriteTreeOption,
                ReadTreeOption,
                CommitOption,
                LogOption,
                CheckoutOption,
                TagOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                (CatFileOption o) => CatFile(o),
                (WriteTreeOption o) => WriteTree(o),
                (ReadTreeOption o) => ReadTree(o),
                (CommitOption o) => Commit(o),
                (LogOption o) => Log(o),
                (CheckoutOption o) => Checkout(o),
                (TagOption o) => CreateTag(o),
                errors => 1) ;
            return exitCode;
        }

        private static int CreateTag(TagOption o)
        {
            string oid = OidConverter(o.Oid) ?? dataProvider.GetRef("HEAD");
            baseOperator.CreateTag(o.Name, oid);
            return 0;
        }

        private static int Checkout(CheckoutOption o)
        {
            baseOperator.Checkout(OidConverter(o.Oid));
            return 0;
        }

        static int Init(InitOption _)
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

        static int CatFile(CatFileOption o)
        {
            byte[] data = dataProvider.GetObject(OidConverter(o.Object));
            if(data.Length > 0)
            {
                Console.WriteLine(data.Decode());
            }
            return 0;
        }

        static int WriteTree(WriteTreeOption _)
        {
            Console.WriteLine(baseOperator.WriteTree());
            return 0;
        }

        static int ReadTree(ReadTreeOption o)
        {
            baseOperator.ReadTree(OidConverter(o.Tree));
            return 0;
        }

        static int Commit(CommitOption o)
        {
            Console.WriteLine(baseOperator.Commit(o.Message));
            return 0;
        }

        static int Log(LogOption o)
        {
            string oid = OidConverter(o.Oid) ?? dataProvider.GetRef("HEAD");
            while(!string.IsNullOrEmpty(oid))
            {
                var commit = baseOperator.GetCommit(oid);
                Console.WriteLine($"commit {oid}");
                Console.WriteLine($"{commit.Message}    ");
                Console.WriteLine("");
                oid = commit.Parent;
            }
            return 0;
        }
    }
}
