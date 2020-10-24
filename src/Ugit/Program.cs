using CommandLine;
using System;
using System.Collections.Generic;
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
                TagOption,
                KOption,
                BranchOption,
                StatusOption,
                ResetOption,
                ShowOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                (CatFileOption o) => CatFile(o),
                (WriteTreeOption o) => WriteTree(o),
                (ReadTreeOption o) => ReadTree(o),
                (CommitOption o) => Commit(o),
                (LogOption o) => Log(o),
                (CheckoutOption o) => Checkout(o),
                (TagOption o) => CreateTag(o),
                (KOption o) => K(o),
                (BranchOption o) => Branch(o),
                (StatusOption o) => Status(o),
                (ResetOption o) => Reset(o),
                (ShowOption o) => Show(o),
                errors => 1); 
            return exitCode;
        }

        private static int Show(ShowOption o)
        {
            string oid = OidConverter(o.Oid);
            if(string.IsNullOrEmpty(oid))
            {
                return 0;
            }

            var commit = baseOperator.GetCommit(oid);
            PrintCommit(oid, commit);
            return 0;
        }

        private static void PrintCommit(string oid, Commit commit, IEnumerable<string> @ref=null)
        {
            string refStr = @ref != null ? $"({string.Join(',', @ref)})" : "";
            Console.WriteLine($"commit {oid}{refStr}\n");
            Console.WriteLine($"{commit.Message}     ");
            Console.WriteLine("");
        }

        private static int Reset(ResetOption o)
        {
            string oid = OidConverter(o.Commit);
            baseOperator.Reset(oid);
            return 0;
        }

        private static int Status(StatusOption _)
        {
            string head = baseOperator.GetOid("@");
            string branch = baseOperator.GetBranchName();
            if(string.IsNullOrEmpty(branch))
            {
                Console.WriteLine($"HEAD detached at {head.Substring(0, 10)}");
            }
            else
            {
                Console.WriteLine($"On branch {branch}");
            }

            return 0;
        }

        private static int K(KOption _)
        {
            string dot = "digraph commits {\n";
            var oids = new HashSet<string>();
            foreach (var (refName, @ref) in dataProvider.IterRefs("", false))
            {
                dot += $"\"{refName}\" [shape=note]\n";
                dot += $"\"{refName}\" -> \"{@ref.Value}\"\n";
                if (!@ref.Symbolic)
                {
                    oids.Add(@ref.Value);
                }
            }

            foreach (var oid in baseOperator.IterCommitsAndParents(oids))
            {
                var commit = baseOperator.GetCommit(oid);
                dot += $"\"{oid}\" [shape=box style=filled label=\"{oid.Substring(0, 10)}\"]\n";
                if (!string.IsNullOrWhiteSpace(commit.Parent))
                {
                    dot += $"\"{oid}\" -> \"{commit.Parent}\"\n";
                }
            }

            dot += "}";
            Console.WriteLine(dot);

            return 0;
        }

        private static int CreateTag(TagOption o)
        {
            string oid = OidConverter(o.Oid);
            baseOperator.CreateTag(o.Name, oid);
            return 0;
        }

        private static int Checkout(CheckoutOption o)
        {
            baseOperator.Checkout(o.Commit);
            return 0;
        }

        static int Init(InitOption _)
        {
            baseOperator.Init();
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
            string oid = OidConverter(o.Oid);

            IDictionary<string, IList<string>> refs = new Dictionary<string, IList<string>>();
            foreach (var (refname, @ref) in dataProvider.IterRefs())
            {
                if(refs.ContainsKey(@ref.Value))
                {
                    refs[@ref.Value].Add(refname);
                }
                else
                {
                    refs[@ref.Value] = new List<string>() { refname };
                }
            }

            foreach (var objectId in baseOperator.IterCommitsAndParents(new string[] { oid }))
            {
                var commit = baseOperator.GetCommit(objectId);
                PrintCommit(objectId, commit, refs.ContainsKey(objectId) ? refs[objectId]: null);
            }

            return 0;
        }

        static int Branch(BranchOption o)
        {
            string startPoint = OidConverter(o.StartPoint);

            if (string.IsNullOrEmpty(o.Name))
            {
                string current = baseOperator.GetBranchName();
                foreach (var branch in baseOperator.IterBranchNames())
                {
                    string prefix = branch == current ? "*" : "";
                    Console.WriteLine($"{prefix}{branch}");
                }
            }
            else
            {
                baseOperator.CreateBranch(o.Name, startPoint);
                Console.WriteLine($"Branch {o.Name} create at {startPoint.Substring(0, 10)}");
            }
            return 0;
        }
    }
}
