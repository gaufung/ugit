using Microsoft.Extensions.Logging;

namespace Tindo.UgitCLI
{
    using CommandLine;
    using Microsoft.Extensions.DependencyInjection;
    using Options;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.IO.Abstractions;
    using System.Net.Http;
    using UgitCore;
    using UgitCore.Operations;

    /// <summary>
    /// The console program.
    /// </summary>
    [ExcludeFromCodeCoverage]
    internal class Program
    {
        private static readonly IDataProvider LocalDataProvider;

        private static readonly IFileSystem FileSystem;

        private static readonly IFileOperator LocalPhysicalFileOperator;

        private static readonly Func<string, string> OidConverter;

        private static readonly IServiceProvider ServiceProvider;

        
        static Program()
        {
            ServiceProvider = new ServiceCollection()
                .AddHttpClient()
                .AddLogging(
                    builder => builder.AddConsole())
                .BuildServiceProvider();

            FileSystem = new FileSystem();
            LocalPhysicalFileOperator = new PhysicalFileOperator(FileSystem);
            LocalDataProvider = new LocalDataProvider(LocalPhysicalFileOperator, 
                ServiceProvider.GetRequiredService<ILoggerFactory>());
            OidConverter = LocalDataProvider.GetOid;
            
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
            IFileOperator remoteFileOperator;
            if (config.Remote.HasValue && config.Remote.Value.Name.Equals(o.Remote,
                StringComparison.OrdinalIgnoreCase))
            {
                IFileOperator fileOperator = new HttpFileOperator(config.Remote.Value.Url,
                    ServiceProvider.GetRequiredService<IHttpClientFactory>(),
                    ServiceProvider.GetRequiredService<ILoggerFactory>());
                remoteDataProvider = new HttpDataProvider(fileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>());
                remoteFileOperator = new HttpFileOperator(config.Remote.Value.Url, ServiceProvider.GetRequiredService<IHttpClientFactory>(),
                   ServiceProvider.GetRequiredService<ILoggerFactory>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), o.Remote, ServiceProvider.GetRequiredService<ILoggerFactory>()); ;
                remoteFileOperator = new PhysicalFileOperator(FileSystem);
            }

            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider, remoteFileOperator, ServiceProvider.GetRequiredService<ILoggerFactory>()), ServiceProvider.GetRequiredService<ILoggerFactory>());

            ITreeOperation treeOperation = new DefaultTreeOperation(LocalDataProvider, LocalPhysicalFileOperator, ServiceProvider.GetRequiredService<ILoggerFactory>());
            ICommitOperation localCommitOperation = new DefaultCommitOperation(LocalDataProvider, treeOperation,
                ServiceProvider.GetRequiredService<ILoggerFactory>());

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                LocalDataProvider, localCommitOperation, remoteDataProvider, remoteCommitOperation, LocalPhysicalFileOperator, remoteFileOperator);
            string refName = Path.Join(Constants.Refs, Constants.Heads, o.Branch);
            remoteOperation.Push(refName);
            return 0;
        }

        private static int Fetch(FetchOption o)
        {
            IDataProvider remoteDataProvider;
            IFileOperator remoteFileOperator = null;
            var config = LocalDataProvider.Config;
            if (config.Remote.HasValue && config.Remote.Value.Name.Equals(o.Remote,
                StringComparison.OrdinalIgnoreCase))
            {
                IFileOperator fileOperator = new HttpFileOperator(config.Remote.Value.Url,
                    ServiceProvider.GetRequiredService<IHttpClientFactory>(),
                    ServiceProvider.GetRequiredService<ILoggerFactory>());
                remoteDataProvider = new HttpDataProvider(fileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>());
                remoteFileOperator = new HttpFileOperator(config.Remote.Value.Url, ServiceProvider.GetRequiredService<IHttpClientFactory>(),
                    ServiceProvider.GetRequiredService<ILoggerFactory>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), o.Remote, ServiceProvider.GetRequiredService<ILoggerFactory>()); ;
                remoteFileOperator = new PhysicalFileOperator(FileSystem);
            }

            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider, remoteFileOperator, ServiceProvider.GetRequiredService<ILoggerFactory>()), ServiceProvider.GetRequiredService<ILoggerFactory>());

            ITreeOperation treeOperation = new DefaultTreeOperation(LocalDataProvider, LocalPhysicalFileOperator, ServiceProvider.GetRequiredService<ILoggerFactory>());
            ICommitOperation localCommitOperation = new DefaultCommitOperation(LocalDataProvider, treeOperation,
                ServiceProvider.GetRequiredService<ILoggerFactory>());

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                LocalDataProvider, localCommitOperation, remoteDataProvider, remoteCommitOperation, LocalPhysicalFileOperator, remoteFileOperator);

            remoteOperation.Fetch();
            return 0;
        }

        private static int Add(AddOption o)
        {
            new DefaultAddOperation(LocalDataProvider, LocalPhysicalFileOperator).Add(o.Files);
            return 0;
        }

        private static int Merge(MergeOption o)
        {
            var commit = OidConverter(o.Commit);
            new DefaultMergeOperation(
                LocalDataProvider, 
                new DefaultCommitOperation(
                    LocalDataProvider, 
                    new DefaultTreeOperation(
                        LocalDataProvider, 
                        LocalPhysicalFileOperator, 
                        ServiceProvider.GetRequiredService<ILoggerFactory>()
                        ), 
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    ),
                new DefaultTreeOperation(
                    LocalDataProvider,
                    LocalPhysicalFileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    ),
                new DefaultDiffOperation(
                    LocalDataProvider,
                    new DefaultDiffProxyOperation(),
                    LocalPhysicalFileOperator
                    )
                ).Merge(commit);
            return 0;
        }

        private static int Different(DiffOption o)
        {
            var commit = OidConverter(o.Commit);
            var commitOperation = new DefaultCommitOperation(
                    LocalDataProvider,
                    new DefaultTreeOperation(
                        LocalDataProvider,
                        LocalPhysicalFileOperator,
                        ServiceProvider.GetRequiredService<ILoggerFactory>()
                        ),
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    );
            var tree = commitOperation.Get(commit).Tree;
            var diff = new DefaultDiffOperation(
                    LocalDataProvider,
                    new DefaultDiffProxyOperation(),
                    LocalPhysicalFileOperator
                    );
            var treeOperation = new DefaultTreeOperation(
                    LocalDataProvider,
                    LocalPhysicalFileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    );
            var result = diff.DiffTrees(treeOperation.GetTree(tree), treeOperation.GetWorkingTree());
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
            var commitOperation = new DefaultCommitOperation(
                   LocalDataProvider,
                   new DefaultTreeOperation(
                       LocalDataProvider,
                       LocalPhysicalFileOperator,
                       ServiceProvider.GetRequiredService<ILoggerFactory>()
                       ),
                   ServiceProvider.GetRequiredService<ILoggerFactory>()
                   );

            var commit = commitOperation.Get(oid);

            string parentTree = null;
            if (commit.Parents.Count > 0)
            {
                parentTree = commitOperation.Get(commit.Parents[0]).Tree;
            }

            PrintCommit(oid, commit);
            var diff = new DefaultDiffOperation(
                   LocalDataProvider,
                   new DefaultDiffProxyOperation(),
                   LocalPhysicalFileOperator
                   );
            var treeOperation = new DefaultTreeOperation(
                    LocalDataProvider,
                    LocalPhysicalFileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    );
            var result = diff.DiffTrees(treeOperation.GetTree(parentTree), treeOperation.GetTree(commit.Tree));
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
            var resetOperation = new DefaultResetOperation(LocalDataProvider);
            resetOperation.Reset(oid);
            return 0;
        }

        private static int Status(StatusOption _)
        {
            string head = OidConverter("@");
            var branchOpeartion = new DefaultBranchOperation(LocalDataProvider);
            string branch = branchOpeartion.Current;
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
            var commitOperation = new DefaultCommitOperation(
                    LocalDataProvider,
                    new DefaultTreeOperation(
                        LocalDataProvider,
                        LocalPhysicalFileOperator,
                        ServiceProvider.GetRequiredService<ILoggerFactory>()
                        ),
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    );
            string headTree = commitOperation.Get(head).Tree;
            bool section = false;
            var diff = new DefaultDiffOperation(
                    LocalDataProvider,
                    new DefaultDiffProxyOperation(),
                    LocalPhysicalFileOperator
                    );
            var treeOperation = new DefaultTreeOperation(
                    LocalDataProvider,
                    LocalPhysicalFileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    );
            foreach (var (path, action) in diff.IterChangedFiles(treeOperation.GetTree(headTree), treeOperation.GetIndexTree()))
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
            foreach (var (path, action) in diff.IterChangedFiles(treeOperation.GetIndexTree(), treeOperation.GetWorkingTree()))
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
            var tagOperation = new DefaultTagOperation(LocalDataProvider);
            if (string.IsNullOrWhiteSpace(o.Oid) && string.IsNullOrWhiteSpace(o.Name))
            {
                foreach (var tag in tagOperation.All)
                {
                    Console.WriteLine(tag);
                }
            }
            else
            {
                string oid = OidConverter(o.Oid);
                tagOperation.Create(o.Name, oid);
            }

            return 0;
        }

        private static int Checkout(CheckoutOption o)
        {
            var checkoutOperation = new DefaultCheckoutOperation(
                LocalDataProvider,
                new DefaultTreeOperation(
                    LocalDataProvider,
                    LocalPhysicalFileOperator,
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    ),
                new DefaultCommitOperation(
                    LocalDataProvider,
                    new DefaultTreeOperation(
                        LocalDataProvider,
                        LocalPhysicalFileOperator,
                        ServiceProvider.GetRequiredService<ILoggerFactory>()
                        ),
                    ServiceProvider.GetRequiredService<ILoggerFactory>()
                    ),
                new DefaultBranchOperation(LocalDataProvider)
                );
            checkoutOperation.Checkout(o.Commit);
            return 0;
        }

        private static int Init(InitOption _)
        {
            new DefaultInitOperation(LocalDataProvider).Init();
            Console.WriteLine($"Initialized empty ugit repository.");
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
            var treeOperation = new DefaultTreeOperation(
                   LocalDataProvider,
                   LocalPhysicalFileOperator,
                   ServiceProvider.GetRequiredService<ILoggerFactory>()
                   );
            treeOperation.ReadTree(OidConverter(o.Tree));
            return 0;
        }

        private static int Commit(CommitOption o)
        {
            try
            {
                var commitOperation = new DefaultCommitOperation(
                      LocalDataProvider,
                      new DefaultTreeOperation(
                          LocalDataProvider,
                          LocalPhysicalFileOperator,
                          ServiceProvider.GetRequiredService<ILoggerFactory>()
                          ),
                      ServiceProvider.GetRequiredService<ILoggerFactory>()
                      );
                Console.WriteLine(commitOperation.Create(o.Message));
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

            var commitOperation = new DefaultCommitOperation(
                   LocalDataProvider,
                   new DefaultTreeOperation(
                       LocalDataProvider,
                       LocalPhysicalFileOperator,
                       ServiceProvider.GetRequiredService<ILoggerFactory>()
                       ),
                   ServiceProvider.GetRequiredService<ILoggerFactory>()
                   );
            foreach (var objectId in commitOperation.GetHistory(new string[] { oid }))
            {
                var commit = commitOperation.Get(objectId);
                PrintCommit(objectId, commit, refs.ContainsKey(objectId) ? refs[objectId] : null);
            }

            return 0;
        }

        private static int Branch(BranchOption o)
        {
            string startPoint = OidConverter(o.StartPoint);
            var branchOperation = new DefaultBranchOperation(LocalDataProvider);
            if (string.IsNullOrEmpty(o.Name))
            {
               
                string current = branchOperation.Current;
                foreach (var branch in branchOperation.Names)
                {
                    string prefix = branch == current ? "*" : string.Empty;
                    Console.WriteLine($"{prefix}{branch}");
                }
            }
            else
            {
                branchOperation.Create(o.Name, startPoint);
                Console.WriteLine($"Branch {o.Name} create at {startPoint.Substring(0, 10)}");
            }

            return 0;
        }
    }
}