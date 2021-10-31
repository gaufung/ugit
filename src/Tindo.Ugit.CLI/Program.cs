using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Abstractions;
using System.Net;
using System.Net.Http;

namespace Tindo.Ugit.CLI
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        private static readonly IDataProvider DataProvider;

        private static readonly IFileSystem FileSystem;

        private static IDiffOperation Diff;

        private static ICommitOperation CommitOperation;

        private static ITreeOperation TreeOperation;

        private static ITagOperation TagOperation;

        private static IResetOperation ResetOperation;

        private static IMergeOperation MergeOperation;

        private static IInitOperation InitOperation;

        private static ICheckoutOperation CheckoutOperation;

        private static IBranchOperation BranchOperation;

        private static IAddOperation AddOperation;

        private static IFileOperator FileOperator;

        private static readonly Func<string, string> OidConverter;

        private static readonly IHttpClientFactory HttpClientFactory;

        static Program()
        {
            FileSystem = new FileSystem();
            FileOperator = new PhysicalFileOperator(FileSystem);
            DataProvider = new LocalDataProvider(FileOperator);
            OidConverter = DataProvider.GetOid;
            HttpClientFactory = new ServiceCollection()
                .AddHttpClient()
                .BuildServiceProvider()
                .GetRequiredService<IHttpClientFactory>();
        }

        private static int Main(string[] args)
        {
            try
            {
                return CreateArgsCommand().Invoke(args);
            }
            catch(UgitException)
            {
                // swalle it
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return 1;
            }
            return 0;
        }

        static Command CreateArgsCommand()
        {
            var initCmd = new Command("init", "Create an empty ugit repository or reinitialize an existing one")
            {
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            initCmd.Handler = CommandHandler.Create<bool>(Init);

            var hashObjectCmd = new Command("hash-object", "hash a file object")
            {
                new Argument<string>("file"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            hashObjectCmd.Handler = CommandHandler.Create<string, bool>(HashObject);

            var catFileCmd = new Command("cat-file", "look up a file by object id")
            {
                new Argument<string>("object"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            catFileCmd.Handler = CommandHandler.Create<string, bool>(CatFile);

            var readTreeCmd = new Command("read-tree", "read out tree")
            {
                new Argument<string>("tree"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            readTreeCmd.Handler = CommandHandler.Create<string, bool>(ReadTree);

            var commitCmd = new Command("commit", "Record changes to the repository")
            {
                new Option<string>(new string[]{"-m", "--message"}, "message"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            commitCmd.Handler = CommandHandler.Create<string, bool>(Commit);

            var logCmd = new Command("log", "Show commit logs")
            {
                new Argument<string>("oid", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            logCmd.Handler = CommandHandler.Create<string, bool>(Log);

            var checkoutCmd = new Command("checkout", "Check out the specific commit")
            {
                new Argument<string>("commit"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            checkoutCmd.Handler = CommandHandler.Create<string, bool>(Checkout);

            var tagCmd = new Command("tag", "Create, list, delete or verify a tag object signed with GPG")
            {
                new Argument<string>("name", ()=>string.Empty),
                new Argument<string>("oid", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            tagCmd.Handler = CommandHandler.Create<string, string, bool>(TagOp);

            var branchCmd = new Command("branch", "List, create, or delete branches")
            {
                new Argument<string>("name", () => string.Empty),
                new Argument<string>("oid", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            branchCmd.Handler = CommandHandler.Create<string, string, bool>(Branch);

            var statusCmd = new Command("status", "Show the current work tree status")
            {
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            statusCmd.Handler = CommandHandler.Create<bool>(Status);

            var resetCmd = new Command("reset", "Reset current HEAD to the specified state")
            {
                new Argument<string>("commit"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            resetCmd.Handler = CommandHandler.Create<string, bool>(Reset);

            var showCmd = new Command("show", "Show the working tree status")
            {
                new Argument<string>("oid", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            showCmd.Handler = CommandHandler.Create<string, bool>(Show);

            var diffCmd = new Command("diff", "Show changes between commits, commit and working tree, etc")
            {
                new Argument<string>("commit", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            diffCmd.Handler = CommandHandler.Create<string, bool>(Different);

            var mergeCmd = new Command("merge", "Join two or more development histories together")
            {
                new Argument<string>("commit", () => "@"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            mergeCmd.Handler = CommandHandler.Create<string, bool>(Merge);

            var addCmd = new Command("add", "Add file contents to the index")
            {
                new Argument<IEnumerable<string>>("files"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            addCmd.Handler = CommandHandler.Create<IEnumerable<string>, bool>(Add);

            var fetchCmd = new Command("fetch", "Download objects and refs from another repository")
            {
                new Argument<string>("remote"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            fetchCmd.Handler = CommandHandler.Create<string, bool>(Fetch);

            var pushCmd = new Command("push", "Update remote refs along with associated objects")
            {
                new Argument<string>("remote"),
                new Argument<string>("branch"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            pushCmd.Handler = CommandHandler.Create<string, string, bool>(Push);


            var remoteCmd = new Command("remote", "Add or update the remote repository")
            {
                new Argument<string>("name"),
                new Argument<string>("url"),
                new Option(new[] { "--verbose", "-v" }, "verbose"),
            };
            remoteCmd.Handler = CommandHandler.Create<string, string, bool>(Remote);

            var authorCmd = new Command("author", "Add or update the author")
            {
                new Argument<string>("name"),
                new Argument<string>("email"),
                new Option(new[] { "--verbose", "-v" }, "verbose")
            };
            authorCmd.Handler = CommandHandler.Create<string, string, bool>(Author);

            var rootCommand = new RootCommand
            {
                initCmd,
                hashObjectCmd,
                catFileCmd,
                readTreeCmd,
                commitCmd,
                logCmd,
                checkoutCmd,
                tagCmd,
                branchCmd,
                statusCmd,
                resetCmd,
                showCmd,
                diffCmd,
                mergeCmd,
                addCmd,
                fetchCmd,
                pushCmd,
                remoteCmd,
                authorCmd,
            };

            return rootCommand;
        }

        private static ILoggerFactory CreateLoggerFactory(bool verbose)
        {
            return LoggerFactory.Create(builder =>
            {
                builder.AddFilter(level =>
                {
                    return
                        verbose ? level >= LogLevel.Debug :
                                    level > LogLevel.Information;
                });
                builder.AddConsole();
            });
        }

        private static void CreateDefaultOperations(bool verbose)
        {
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            InitOperation = new InitOperation(DataProvider, loggerFactory.CreateLogger<InitOperation>());
            Diff = new DiffOperation(DataProvider, new DiffProxy(), loggerFactory.CreateLogger<DiffOperation>());
            TreeOperation = new TreeOperation(DataProvider, loggerFactory.CreateLogger<TreeOperation>());
            CommitOperation = new CommitOperation(DataProvider, TreeOperation, loggerFactory.CreateLogger<CommitOperation>());
            TagOperation = new TagOperation(DataProvider, loggerFactory.CreateLogger<TagOperation>());
            ResetOperation = new ResetOperation(DataProvider, loggerFactory.CreateLogger<ResetOperation>());
            MergeOperation = new MergeOperation(DataProvider, CommitOperation, TreeOperation, Diff, loggerFactory.CreateLogger<MergeOperation>());
            BranchOperation = new BranchOperation(DataProvider, loggerFactory.CreateLogger<BranchOperation>());
            CheckoutOperation = new CheckoutOperation(DataProvider, TreeOperation, CommitOperation, BranchOperation, loggerFactory.CreateLogger<CheckoutOperation>());
            AddOperation = new AddOperation(DataProvider, loggerFactory.CreateLogger<AddOperation>());

        }

        private static void Push(string remote, string branch, bool verbose)
        {
            CreateDefaultOperations(verbose);
            Config config = DataProvider.Config;
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            IDataProvider remoteDataProvider;
            if (config.Remote != null && config.Remote.Name.Equals(remote, StringComparison.OrdinalIgnoreCase))
            {
                remoteDataProvider = new HttpDataProvider(
                    new HttpFileOperator(config.Remote.Url, HttpClientFactory, loggerFactory.CreateLogger<HttpFileOperator>()),
                    loggerFactory.CreateLogger<HttpDataProvider>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), remote);
            }
            ICommitOperation remoteCommitOperation = new CommitOperation(remoteDataProvider, new TreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new RemoteOperation(
                DataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation,
                loggerFactory.CreateLogger<RemoteOperation>());
            string refName = Path.Join("refs", "heads", branch);
            remoteOperation.Push(refName);
        }

        private static void Fetch(string remote, bool verbose)
        {
            CreateDefaultOperations(verbose);
            Config config = DataProvider.Config;
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            IDataProvider remoteDataProvider;
            if (config.Remote != null && config.Remote.Name.Equals(remote, StringComparison.OrdinalIgnoreCase))
            {
                remoteDataProvider = new HttpDataProvider(
                    new HttpFileOperator(config.Remote.Url, HttpClientFactory, loggerFactory.CreateLogger<HttpFileOperator>()),
                    loggerFactory.CreateLogger<HttpDataProvider>());
            }
            else
            {
                remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), remote);
            }
            ICommitOperation remoteCommitOperation = new CommitOperation(remoteDataProvider, new TreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new RemoteOperation(
                DataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation,
                loggerFactory.CreateLogger<RemoteOperation>());
            remoteOperation.Fetch();
        }


        private static void Add(IEnumerable<string> files, bool verbose)
        {
            CreateDefaultOperations(verbose);
            AddOperation.Add(files);
        }

        private static void Merge(string commit, bool verbose)
        {
            CreateDefaultOperations(verbose);
            commit = OidConverter(commit);
            MergeOperation.Merge(commit);
        }

        private static void Different(string commit, bool verbose)
        {
            CreateDefaultOperations(verbose);
            commit = OidConverter(commit);
            var tree = CommitOperation.GetCommit(commit).Tree;
            var result = Diff.DiffTrees(TreeOperation.GetTree(tree), TreeOperation.GetWorkingTree());
            Console.WriteLine(result);
        }

        private static void Show(string oid, bool verbose)
        {
            CreateDefaultOperations(verbose);
            oid = OidConverter(oid);
            if (string.IsNullOrEmpty(oid))
            {
                return;
            }

            var commit = CommitOperation.GetCommit(oid);

            string parentTree = null;
            if (commit.Parents.Count > 0)
            {
                parentTree = CommitOperation.GetCommit(commit.Parents[0]).Tree;
            }

            PrintCommit(oid, commit);
            var result = Diff.DiffTrees(TreeOperation.GetTree(parentTree), TreeOperation.GetTree(commit.Tree));
            Console.WriteLine(result);
        }

        private static void PrintCommit(string oid, Commit commit, IEnumerable<string> @ref = null)
        {
            string refStr = @ref != null ? $"({string.Join(',', @ref)})" : string.Empty;
            Console.WriteLine($"commit {oid}{refStr}");
            if (commit.Author != null)
            {
                Console.WriteLine($"author {commit.Author}\n");
            }
            else
            {
                Console.WriteLine($"author unknown\n");
            }
            Console.WriteLine($"{commit.Message}     ");
            Console.WriteLine(string.Empty);
        }

        private static void Reset(string commit, bool verbose)
        {
            CreateDefaultOperations(verbose);
            string oid = OidConverter(commit);
            ResetOperation.Reset(oid);
        }

        private static void Status(bool verbose)
        {
            CreateDefaultOperations(verbose);
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

            string mergeHead = DataProvider.GetRef("MERGE_HEAD").Value;
            if (!string.IsNullOrEmpty(mergeHead))
            {
                Console.WriteLine($"Merging with {mergeHead.Substring(0, 10)}");
            }

            string headTree = CommitOperation.GetCommit(head).Tree;
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
        }

        static void TagOp(string name, string oid, bool verbose)
        {
            CreateDefaultOperations(verbose);
            if (string.IsNullOrWhiteSpace(name))
            {
                foreach (var tag in TagOperation.All)
                {
                    Console.WriteLine(tag);
                }
            }
            else
            {
                oid = OidConverter(oid);
                TagOperation.Create(name, oid);
            }
        }

        private static void Checkout(string commit, bool verbose)
        {
            CreateDefaultOperations(verbose);
            CheckoutOperation.Checkout(commit);
        }


        private static void Init(bool verbose)
        {
            CreateDefaultOperations(verbose);
            InitOperation.Init();
            Console.WriteLine($"Initialized empty ugit repository in {DataProvider.GitDirFullPath}");
        }

        private static void HashObject(string file, bool verbose)
        {
            CreateDefaultOperations(verbose);
            byte[] data = FileSystem.File.ReadAllBytes(file);
            Console.WriteLine(DataProvider.WriteObject(data));
        }

        private static void CatFile(string oid, bool verbose)
        {
            CreateDefaultOperations(verbose);
            byte[] data = DataProvider.GetObject(OidConverter(oid));
            if (data.Length > 0)
            {
                Console.WriteLine(data.Decode());
            }
        }

        private static void ReadTree(string tree, bool verbose)
        {
            CreateDefaultOperations(verbose);
            TreeOperation.ReadTree(OidConverter(tree));
        }

        private static void Commit(string message, bool verbose)
        {
            CreateDefaultOperations(verbose);
            try
            {
                Console.WriteLine(CommitOperation.CreateCommit(message));
            }
            catch (UgitException e)
            {
                Console.WriteLine(e.Message);
            }
        }

        static void Log(string oid, bool verbose)
        {
            CreateDefaultOperations(verbose);
            oid = OidConverter(oid);

            IDictionary<string, IList<string>> refs = new Dictionary<string, IList<string>>();
            foreach (var (refname, @ref) in DataProvider.GetAllRefs())
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

            foreach (var objectId in CommitOperation.GetCommitHistory(new string[] { oid }))
            {
                var commit = CommitOperation.GetCommit(objectId);
                PrintCommit(objectId, commit, refs.ContainsKey(objectId) ? refs[objectId] : null);
            }
        }

        private static void Branch(string name, string oid, bool verbose)
        {
            CreateDefaultOperations(verbose);
            string startPoint = OidConverter(oid);
            if (string.IsNullOrEmpty(name))
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
                BranchOperation.Create(name, startPoint);
                Console.WriteLine($"Branch {name} create at {startPoint.Substring(0, 10)}");
            }
        }

        private static void Remote(string name, string url, bool verbose)
        {
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            var config = DataProvider.Config;
            config.Remote = new Remote(name, url);
            DataProvider.Config = config;
        }

        private static void Author(string name, string email, bool verbose)
        {
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose);
            var config = DataProvider.Config;
            config.Author = new Author(name, email);
            DataProvider.Config = config;
        }
    }
}
