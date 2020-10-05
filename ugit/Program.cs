using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Runtime.InteropServices;
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
                ReadTreeOptions,
                CommitOptions,
                LogOptions,
                CheckOutOptions,
                TagOptions,
                KOptions,
                BranchOptions,
            StatusOptions>(args).MapResult(
                (InitOptions o) => Init(o),
                (HashObjectOptions o) => HashObject(o),
                (CatFileOptions o) => CatFile(o),
                (WriteTreeOptions o) => WriteTree(o),
                (ReadTreeOptions o) => ReadTree(o),
                (CommitOptions o) => Commit(o),
                (LogOptions o) => Log(o),
                (CheckOutOptions o) => CheckOut(o),
                (TagOptions o) => Tag(o),
                (KOptions o) => K(o),
                (BranchOptions o) => Branch(o),
                (StatusOptions o) => Status(o),
                errors => 1);
            return exitCode;
        }

        static int Init(InitOptions o)
        {
            _base.Init();
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
            string oid = _base.GetOid(o.Object);
            Console.Write(_data.GetObject(oid, null).Decode());
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
            string tree = _base.GetOid(o.Tree);
            _base.ReadTree(tree);
            return 0;
        }

        static int Commit(CommitOptions o)
        {
            Console.WriteLine(_base.Commit(o.Message));
            return 0;
        }

        static int Log(LogOptions o)
        {
            IDictionary<string, List<string>> refs = new Dictionary<string, List<string>>();
            foreach (var (refName,@ref) in _data.IterRefs())
            {
                if (refs.ContainsKey(@ref.Value))
                {
                    refs[@ref.Value].Add(refName);
                }
                else
                {
                    refs[@ref.Value] = new List<string>(){refName};
                }
            }
            string oid = _base.GetOid(o.Oid);
            foreach (var objectId in _base.IterCommitAndParents(new[] {oid}))
            {
                var commit = _base.GetCommit(objectId);
                string refsString = refs.ContainsKey(objectId) ? $"({string.Join(", ", refs[oid])})" : "";
                Console.WriteLine($"commit {oid}{refsString}\n");
                Console.WriteLine($"{commit.Message}    ");
                Console.WriteLine("");
            }

            return 0;
        }

        static int CheckOut(CheckOutOptions o)
        {
            _base.CheckOut(o.Commit);
            return 0;
        }

        static int Tag(TagOptions o)
        {
            string oid = _base.GetOid(o.Oid);
            _base.CreateTag(o.Name, oid);
            return 0;
        }

        static int Branch(BranchOptions o)
        {
            if (string.IsNullOrWhiteSpace(o.Name))
            {
                string current = _base.GetBranchName();
                foreach (string branch in _base.IterBranchName())
                {
                    string prefix = branch == current ? "*" : " ";
                    Console.WriteLine($"{prefix} {branch}");
                }
            }
            else
            {
                string startPoint = _base.GetOid(o.StartPoint);
                _base.CreateBranch(o.Name, startPoint);
                Console.WriteLine($"Branch {o.Name}, created at {startPoint.Substring(0, 10)}");
            }
            return 0;
        }

        static int K(KOptions k)
        {
            string dot = "digraph commits {\n";
            HashSet<string> oids = new HashSet<string>();
            foreach (var (refname, @ref) in _data.IterRefs("",false))
            {
                dot += $"\"{refname}\" [shape=note]\n";
                dot += $"\"{refname}\" -> \"{@ref}\"";
                if (!@ref.Symbolic)
                {
                    oids.Add(@ref.Value);
                }
            }

            foreach (var oid in _base.IterCommitAndParents(oids))
            {
                var commit = _base.GetCommit(oid);
                dot += $"\"{oid}\" [shape=box style=filled label=\"{oid.Substring(0, 10)}\"]\n";
                if (!string.IsNullOrWhiteSpace(commit.Parent))
                {
                    dot += $"\"{oid}\" -> \"{commit.Parent}\"";
                }
            }

            dot += "}";
            Console.WriteLine(dot);
            return 0;
        }

        static int Status(StatusOptions o)
        {
            string head = _base.GetOid("@");
            string branch = _base.GetBranchName();
            if (!string.IsNullOrWhiteSpace(branch))
            {
                Console.WriteLine($"On branch {branch}");
            }
            else
            {
                Console.WriteLine($"HEAD detached at {head.Substring(0, 10)}");
            }
            return 0;
        }
    }
}