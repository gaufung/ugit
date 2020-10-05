using System;
using System.IO.Abstractions;
using CommandLine;

namespace ugit
{
    class Program
    {
        private static Data _data;
        private static Base _base;

        private static IFileSystem _fileSystem;
        
        static int Main(string[] args)
        {
            _fileSystem = new FileSystem();
            _data = new Data(_fileSystem);
            _base = new Base(_data, _fileSystem);
            int exitCode = Parser.Default.ParseArguments<
                    InitOptions,
            HashObjectOptions,
            CatFileOptions,
            WriteTreeOptions,
            ReadTreeOptions>(args).MapResult(
                    (InitOptions o) => Init(o),
                    (HashObjectOptions o) => HashObject(o),
                    (CatFileOptions o) => CatFile(o),
                    (WriteTreeOptions o) => WriteTree(o),
                    (ReadTreeOptions o) => ReadTree(o),
                    errors => 1);
            return exitCode;
        }

        static int Init(InitOptions o)
        {
            _data.Init();
            Console.WriteLine($"Initialized empty ugit repository in {_data.GitDirPath}");
            return 0;
        }

        static int HashObject(HashObjectOptions o)
        {
            string oid = _data.HashObject(_fileSystem.File.ReadAllBytes(o.File));
            Console.WriteLine(oid);
            return 0;
        }

        static int CatFile(CatFileOptions o)
        {
            Console.Write(_data.GetObject(o.Object, null).Decode());
            return 0;
        }

        static int WriteTree(WriteTreeOptions o)
        {
            string oid = _base.WriteTree();
            Console.WriteLine(oid);
            return 0;
        }

        static int ReadTree(ReadTreeOptions o)
        {
            _base.ReadTree(o.Tree);
            return 0;
        }
    }
}