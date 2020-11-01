namespace Ugit
{
    using System;
    using System.Collections.Generic;
    using System.IO.Abstractions;
    using CommandLine;
    using Ugit.Operations;
    using Ugit.Options;

    /// <summary>
    /// The console program.
    /// </summary>
    internal class Program
    {
        private static readonly IDataProvider DataProvider;

        private static readonly IFileSystem FileSystem;

        private static readonly IBaseOperator BaseOperator;

        private static readonly IDiff Diff;

        private static readonly ICommitOperation CommitOperation;

        private static readonly ITreeOperation TreeOperation;

        private static readonly Func<string, string> OidConverter;

        static Program()
        {
            FileSystem = new FileSystem();
            DataProvider = new DefaultDataProvider();
            Diff = new DefaultDiff(DataProvider, new DefaultDiffProxy(), FileSystem);
            BaseOperator = new DefaultBaseOperator(FileSystem, DataProvider, Diff);
            TreeOperation = new DefaultTreeOperation(DataProvider);
            CommitOperation = new DefaultCommitOperation(DataProvider, TreeOperation);
            OidConverter = BaseOperator.GetOid;
        }

        private static int Main(string[] args)
        {
            int exitCode = Parser.Default.ParseArguments<
                InitOption,
                HashObjectOption,
                CatFileOption,
                ReadTreeOption,
                CommitOption,
                LogOption,
                CheckoutOption,
                TagOption,
                KOption,
                BranchOption,
                StatusOption,
                ResetOption,
                ShowOption,
                DiffOption,
                MergeOption,
                MergeBaseOption,
                AddOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                (CatFileOption o) => CatFile(o),
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
                (DiffOption o) => Different(o),
                (MergeOption o) => Merge(o),
                (MergeBaseOption o) => MergeBase(o),
                (AddOption o) => Add(o),
                errors => 1);
            return exitCode;
        }

        private static int Add(AddOption o)
        {
            BaseOperator.Add(o.Files);
            return 0;
        }

        private static int MergeBase(MergeBaseOption o)
        {
            string commit1 = OidConverter(o.Commit1);
            string commit2 = OidConverter(o.Commit2);
            Console.WriteLine(BaseOperator.GetMergeBase(commit1, commit2));
            return 0;
        }

        private static int Merge(MergeOption o)
        {
            BaseOperator.Merge(o.Commit);
            return 0;
        }

        private static int Different(DiffOption o)
        {
            var commit = OidConverter(o.Commit);
            var tree = BaseOperator.GetCommit(commit).Tree;
            var result = Diff.DiffTrees(BaseOperator.GetTree(tree), BaseOperator.GetWorkingTree());
            Console.WriteLine(result);
            return 0;
        }

        private static int Show(ShowOption o)
        {
            string oid = OidConverter(o.Oid);
            if (string.IsNullOrEmpty(oid))
            {
                return 0;
            }

            var commit = BaseOperator.GetCommit(oid);

            string parentTree = null;
            if (commit.Parents.Count > 0)
            {
                parentTree = BaseOperator.GetCommit(commit.Parents[0]).Tree;
            }

            PrintCommit(oid, commit);
            var result = Diff.DiffTrees(BaseOperator.GetTree(parentTree), BaseOperator.GetTree(commit.Tree));
            Console.WriteLine(result);
            return 0;
        }

        private static void PrintCommit(string oid, Commit commit, IEnumerable<string> @ref = null)
        {
            string refStr = @ref != null ? $"({string.Join(',', @ref)})" : string.Empty;
            Console.WriteLine($"commit {oid}{refStr}\n");
            Console.WriteLine($"{commit.Message}     ");
            Console.WriteLine(string.Empty);
        }

        private static int Reset(ResetOption o)
        {
            string oid = OidConverter(o.Commit);
            BaseOperator.Reset(oid);
            return 0;
        }

        private static int Status(StatusOption _)
        {
            string head = BaseOperator.GetOid("@");
            string branch = BaseOperator.GetBranchName();
            if (string.IsNullOrEmpty(branch))
            {
                Console.WriteLine($"HEAD detached at {head.Substring(0, 10)}");
            }
            else
            {
                Console.WriteLine($"On branch {branch}");
            }

            string mergeHead = DataProvider.GetRef("MERGE_HEAD").Value;
            if (!string.IsNullOrEmpty(mergeHead))
            {
                Console.WriteLine($"Merging with {mergeHead.Substring(0, 10)}");
            }

            Console.WriteLine("\nChanges to be committed:\n");
            string headTree = BaseOperator.GetCommit(head).Tree;
            foreach (var (path, action) in Diff.IterChangedFiles(BaseOperator.GetTree(headTree), BaseOperator.GetIndexTree()))
            {
                Console.WriteLine($"{action}   : {path}");
            }

            Console.WriteLine("\nChanges not staged for commit:\n");
            foreach (var (path, action) in Diff.IterChangedFiles(BaseOperator.GetIndexTree(), BaseOperator.GetWorkingTree()))
            {
                Console.WriteLine($"{action}   : {path}");
            }

            return 0;
        }

        private static int K(KOption _)
        {
            string dot = "digraph commits {\n";
            var oids = new HashSet<string>();
            foreach (var (refName, @ref) in DataProvider.IterRefs(string.Empty, false))
            {
                dot += $"\"{refName}\" [shape=note]\n";
                dot += $"\"{refName}\" -> \"{@ref.Value}\"\n";
                if (!@ref.Symbolic)
                {
                    oids.Add(@ref.Value);
                }
            }

            foreach (var oid in BaseOperator.IterCommitsAndParents(oids))
            {
                var commit = BaseOperator.GetCommit(oid);
                dot += $"\"{oid}\" [shape=box style=filled label=\"{oid.Substring(0, 10)}\"]\n";
                foreach (var parent in commit.Parents)
                {
                    dot += $"\"{oid}\" -> \"{parent}\"\n";
                }
            }

            dot += "}";
            Console.WriteLine(dot);

            return 0;
        }

        private static int CreateTag(TagOption o)
        {
            string oid = OidConverter(o.Oid);
            BaseOperator.CreateTag(o.Name, oid);
            return 0;
        }

        private static int Checkout(CheckoutOption o)
        {
            BaseOperator.Checkout(o.Commit);
            return 0;
        }

        private static int Init(InitOption _)
        {
            BaseOperator.Init();
            Console.WriteLine($"Initialized empty ugit repository in {DataProvider.GitDirFullPath}");
            return 0;
        }

        private static int HashObject(HashObjectOption o)
        {
            byte[] data = FileSystem.File.ReadAllBytes(o.File);
            Console.WriteLine(DataProvider.HashObject(data));
            return 0;
        }

        private static int CatFile(CatFileOption o)
        {
            byte[] data = DataProvider.GetObject(OidConverter(o.Object));
            if (data.Length > 0)
            {
                Console.WriteLine(data.Decode());
            }

            return 0;
        }

        private static int ReadTree(ReadTreeOption o)
        {
            BaseOperator.ReadTree(OidConverter(o.Tree));
            return 0;
        }

        private static int Commit(CommitOption o)
        {
            Console.WriteLine(CommitOperation.CreateCommit(o.Message));
            return 0;
        }

        private static int Log(LogOption o)
        {
            string oid = OidConverter(o.Oid);

            IDictionary<string, IList<string>> refs = new Dictionary<string, IList<string>>();
            foreach (var (refname, @ref) in DataProvider.IterRefs())
            {
                if (refs.ContainsKey(@ref.Value))
                {
                    refs[@ref.Value].Add(refname);
                }
                else
                {
                    refs[@ref.Value] = new List<string>() { refname };
                }
            }

            foreach (var objectId in BaseOperator.IterCommitsAndParents(new string[] { oid }))
            {
                var commit = BaseOperator.GetCommit(objectId);
                PrintCommit(objectId, commit, refs.ContainsKey(objectId) ? refs[objectId] : null);
            }

            return 0;
        }

        private static int Branch(BranchOption o)
        {
            string startPoint = OidConverter(o.StartPoint);

            if (string.IsNullOrEmpty(o.Name))
            {
                string current = BaseOperator.GetBranchName();
                foreach (var branch in BaseOperator.IterBranchNames())
                {
                    string prefix = branch == current ? "*" : string.Empty;
                    Console.WriteLine($"{prefix}{branch}");
                }
            }
            else
            {
                BaseOperator.CreateBranch(o.Name, startPoint);
                Console.WriteLine($"Branch {o.Name} create at {startPoint.Substring(0, 10)}");
            }

            return 0;
        }
    }
}
