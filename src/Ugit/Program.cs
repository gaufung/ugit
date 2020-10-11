﻿using CommandLine;
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

        static Program()
        {
            fileSystem = new FileSystem();
            dataProvider = new DataProvider();
            baseOperator = new BaseOperator(fileSystem, dataProvider);
        }

        static int Main(string[] args)
        {
            int exitCode = Parser.Default.ParseArguments<
                InitOption,
                HashObjectOption,
                CatFileOption,
                WriteTreeOption,
                ReadTreeOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                (CatFileOption o) => CatFile(o),
                (WriteTreeOption o) => WriteTree(o),
                (ReadTreeOption o) => ReadTree(o),
                errors => 1);
            return exitCode;
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
            byte[] data = dataProvider.GetObject(o.Object);
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
            baseOperator.ReadTree(o.Tree);
            return 0;
        }
    }
}