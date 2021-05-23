namespace Tindo.UgitCLI
{
    using System.Net.Http;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using CommandLine;
    using UgitCore.Operations;
    using UgitCore;
    using Options;

    /// <summary>
    /// The console program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        private static readonly IDataProvider LocalDataProvider;

        private static readonly IFileSystem FileSystem;

        private static readonly IDiffOperation Diff;

        private static readonly ICommitOperation CommitOperation;

        private static readonly ITreeOperation TreeOperation;

        private static readonly ITagOperation TagOperation;

        private static readonly IResetOperation ResetOperation;

        private static readonly IMergeOperation MergeOperation;

        private static readonly IInitOperation InitOperation;

        private static readonly ICheckoutOperation CheckoutOperation;

        private static readonly IBranchOperation BranchOperation;

        private static readonly IAddOperation AddOperation;

        private static readonly Func<string, string> OidConverter;

        private static IServiceProvider serviceProvider;

        
        static Program()
        {
            FileSystem = new FileSystem();
            LocalDataProvider = new LocalDataProvider();
            Diff = new DefaultDiffOperation(LocalDataProvider, new DefaultDiffProxyOperation());
            TreeOperation = new DefaultTreeOperation(LocalDataProvider);
            CommitOperation = new DefaultCommitOperation(LocalDataProvider, TreeOperation);
            TagOperation = new DefaultTagOperation(LocalDataProvider);
            ResetOperation = new DefaultResetOperation(LocalDataProvider);
            MergeOperation = new DefaultMergeOperation(LocalDataProvider, CommitOperation, TreeOperation, Diff);
            InitOperation = new DefaultInitOperation(LocalDataProvider);
            BranchOperation = new DefaultBranchOperation(LocalDataProvider);
            CheckoutOperation = new DefaultCheckoutOperation(LocalDataProvider, TreeOperation, CommitOperation, BranchOperation);
            AddOperation = new DefaultAddOperation(LocalDataProvider);
            OidConverter = LocalDataProvider.GetOid;
            serviceProvider = new ServiceCollection()
                .AddHttpClient().BuildServiceProvider();
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
                BranchOption,
                StatusOption,
                ResetOption,
                ShowOption,
                DiffOption,
                MergeOption,
                AddOption,
                FetchOption,
                PushOption,
                ConfigOption,
            RemoteUrlOption>(args).MapResult(
                (InitOption o) => Init(o),
                (HashObjectOption o) => HashObject(o),
                (CatFileOption o) => CatFile(o),
                (ReadTreeOption o) => ReadTree(o),
                (CommitOption o) => Commit(o),
                (LogOption o) => Log(o),
                (CheckoutOption o) => Checkout(o),
                (TagOption o) => TagOp(o),
                (BranchOption o) => Branch(o),
                (StatusOption o) => Status(o),
                (ResetOption o) => Reset(o),
                (ShowOption o) => Show(o),
                (DiffOption o) => Different(o),
                (MergeOption o) => Merge(o),
                (AddOption o) => Add(o),
                (FetchOption o) => Fetch(o),
                (PushOption o) => Push(o),
                (ConfigOption o) => Config(o),
                (RemoteUrlOption o) => SetUrlConfig(o),
                errors => 1);
            return exitCode;
        }

        private static int SetUrlConfig(RemoteUrlOption o)
        {
            Remote remote = new Remote() {Name = o.Name, Url = o.Url};
            var config = LocalDataProvider.Config;
            config.Remote = remote;
            LocalDataProvider.Config = config;
            return 0;
        }

        private static int Config(ConfigOption o)
        {
            var config = LocalDataProvider.Config;
            if (string.IsNullOrEmpty(o.Email) && string.IsNullOrWhiteSpace(o.Name))
            {
                if (config.Author.HasValue)
                {
                    Console.WriteLine($"{config.Author.Value.Name}\n{config.Author.Value.Email}");
                }

                return 0;
            }
            config.Author = new Author() { Email = o.Email, Name = o.Name };
            LocalDataProvider.Config = config;
            return 0;
        }

        private static int Push(PushOption o)
        {
            IDataProvider remoteDataProvider;
            var config = LocalDataProvider.Config;
            if (config.Remote.HasValue && config.Remote.Value.Name.Equals(o.Remote, 
                StringComparison.OrdinalIgnoreCase))
            {
                remoteDataProvider = new HttpDataProvider(config.Remote.Value.Url, 
                    serviceProvider.GetRequiredService<IHttpClientFactory>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new FileSystem(), o.Remote);
            }
            
            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                LocalDataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation);
            string refName = Path.Join(Constants.Refs, Constants.Heads, o.Branch);
            remoteOperation.Push(refName);
            return 0;
        }

        private static int Fetch(FetchOption o)
        {
            IDataProvider remoteDataProvider;
            var config = LocalDataProvider.Config;
            if (config.Remote.HasValue && config.Remote.Value.Name.Equals(o.Remote, 
                StringComparison.OrdinalIgnoreCase))
            {
                remoteDataProvider = new HttpDataProvider(config.Remote.Value.Url, 
                    serviceProvider.GetRequiredService<IHttpClientFactory>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new FileSystem(), o.Remote);
            }
            
            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                LocalDataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation);
            remoteOperation.Fetch();
            return 0;
        }

        private static int Add(AddOption o)
        {
            AddOperation.Add(o.Files);
            return 0;
        }

        private static int Merge(MergeOption o)
        {
            var commit = OidConverter(o.Commit);
            MergeOperation.Merge(commit);
            return 0;
        }

        private static int Different(DiffOption o)
        {
            var commit = OidConverter(o.Commit);
            var tree = CommitOperation.Get(commit).Tree;
            var result = Diff.DiffTrees(TreeOperation.GetTree(tree), TreeOperation.GetWorkingTree());
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

            var commit = CommitOperation.Get(oid);

            string parentTree = null;
            if (commit.Parents.Count > 0)
            {
                parentTree = CommitOperation.Get(commit.Parents[0]).Tree;
            }

            PrintCommit(oid, commit);
            var result = Diff.DiffTrees(TreeOperation.GetTree(parentTree), TreeOperation.GetTree(commit.Tree));
            Console.WriteLine(result);
            return 0;
        }

        private static void PrintCommit(string oid, Commit commit, IEnumerable<string> @ref = null)
        {
            string refStr = @ref != null ? $"({string.Join(',', @ref)})" : string.Empty;
            Console.WriteLine($"commit {oid}{refStr}");
            Console.WriteLine($"Author: {commit.Author}\n");
            Console.WriteLine($"{commit.Message}     ");
            Console.WriteLine(string.Empty);
        }

        private static int Reset(ResetOption o)
        {
            string oid = OidConverter(o.Commit);
            ResetOperation.Reset(oid);
            return 0;
        }

        private static int Status(StatusOption _)
        {
            string head = OidConverter("@");
            string branch = BranchOperation.Current;
            if (string.IsNullOrEmpty(branch))
            {
                Console.WriteLine($"HEAD detached at {head.Substring(0, 10)}");
            }
            else
            {
                Console.WriteLine($"On branch {branch}");
            }

            string mergeHead = LocalDataProvider.GetRef("MERGE_HEAD").Value;
            if (!string.IsNullOrEmpty(mergeHead))
            {
                Console.WriteLine($"Merging with {mergeHead.Substring(0, 10)}");
            }

            string headTree = CommitOperation.Get(head).Tree;
            bool section = false;
            foreach (var (path, action) in Diff.IterChangedFiles(TreeOperation.GetTree(headTree), TreeOperation.GetIndexTree()))
            {
                if (!section)
                {
                    Console.WriteLine("\nChanges to be committed:");
                    section = true;
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"{action}: {path}");
            }

            Console.ResetColor();
            section = false;
            foreach (var (path, action) in Diff.IterChangedFiles(TreeOperation.GetIndexTree(), TreeOperation.GetWorkingTree()))
            {
                if (!section)
                {
                    Console.WriteLine("\nChanges not staged for commit:");
                    section = true;
                }

                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{action}: {path}");
            }

            Console.ResetColor();
            return 0;
        }

        private static int TagOp(TagOption o)
        {
            if (string.IsNullOrWhiteSpace(o.Oid) && string.IsNullOrWhiteSpace(o.Name))
            {
                foreach (var tag in TagOperation.All)
                {
                    Console.WriteLine(tag);
                }
            }
            else
            {
                string oid = OidConverter(o.Oid);
                TagOperation.Create(o.Name, oid);
            }

            return 0;
        }

        private static int Checkout(CheckoutOption o)
        {
            CheckoutOperation.Checkout(o.Commit);
            return 0;
        }

        private static int Init(InitOption _)
        {
            InitOperation.Init();
            Console.WriteLine($"Initialized empty ugit repository in {LocalDataProvider.GitDirFullPath}");
            return 0;
        }

        private static int HashObject(HashObjectOption o)
        {
            byte[] data = FileSystem.File.ReadAllBytes(o.File);
            Console.WriteLine(LocalDataProvider.HashObject(data));
            return 0;
        }

        private static int CatFile(CatFileOption o)
        {
            byte[] data = LocalDataProvider.GetObject(OidConverter(o.Object));
            if (data.Length > 0)
            {
                Console.WriteLine(data.Decode());
            }

            return 0;
        }

        private static int ReadTree(ReadTreeOption o)
        {
            TreeOperation.ReadTree(OidConverter(o.Tree));
            return 0;
        }

        private static int Commit(CommitOption o)
        {
            try
            {
                Console.WriteLine(CommitOperation.Create(o.Message));
            }
            catch (UgitException e)
            {
                Console.WriteLine(e.Message);
            }

            return 0;
        }

        private static int Log(LogOption o)
        {
            string oid = OidConverter(o.Oid);

            IDictionary<string, IList<string>> refs = new Dictionary<string, IList<string>>();
            foreach (var (refname, @ref) in LocalDataProvider.GetAllRefs())
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

            foreach (var objectId in CommitOperation.GetHistory(new string[] { oid }))
            {
                var commit = CommitOperation.Get(objectId);
                PrintCommit(objectId, commit, refs.ContainsKey(objectId) ? refs[objectId] : null);
            }

            return 0;
        }

        private static int Branch(BranchOption o)
        {
            string startPoint = OidConverter(o.StartPoint);

            if (string.IsNullOrEmpty(o.Name))
            {
                string current = BranchOperation.Current;
                foreach (var branch in BranchOperation.Names)
                {
                    string prefix = branch == current ? "*" : string.Empty;
                    Console.WriteLine($"{prefix}{branch}");
                }
            }
            else
            {
                BranchOperation.Create(o.Name, startPoint);
                Console.WriteLine($"Branch {o.Name} create at {startPoint.Substring(0, 10)}");
            }

            return 0;
        }
    }
}