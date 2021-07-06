using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using Tindo.Ugit.Operations;
using System.CommandLine;
using System.CommandLine.Invocation;

namespace Tindo.Ugit.CLI
{
    class Program
    {
        private static readonly IDataProvider DataProvider;

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

        private static readonly IFileOperator FileOperator;

        private static readonly Func<string, string> OidConverter;

        static Program()
        {
            FileSystem = new FileSystem();
            FileOperator = new PhysicalFileOperator(FileSystem);
            DataProvider = new LocalDataProvider(FileOperator);
            Diff = new DefaultDiffOperation(DataProvider, new DefaultDiffProxyOperation());
            TreeOperation = new DefaultTreeOperation(DataProvider);
            CommitOperation = new DefaultCommitOperation(DataProvider, TreeOperation);
            TagOperation = new DefaultTagOperation(DataProvider);
            ResetOperation = new DefaultResetOperation(DataProvider);
            MergeOperation = new DefaultMergeOperation(DataProvider, CommitOperation, TreeOperation, Diff);
            InitOperation = new DefaultInitOperation(DataProvider);
            BranchOperation = new DefaultBranchOperation(DataProvider);
            CheckoutOperation = new DefaultCheckoutOperation(DataProvider, TreeOperation, CommitOperation, BranchOperation);
            AddOperation = new DefaultAddOperation(DataProvider);
            OidConverter = DataProvider.GetOid;
        }

        private static int Main(string[] args)
        {
            return CreateArgsCommand().Invoke(args);
        }

        static Command CreateArgsCommand()
        {
            var initCmd = new Command("init", "initialize ugit repo");
            initCmd.Handler = CommandHandler.Create(Init);

            var hashObjectCmd = new Command("hash-object", "hash a file object")
            {
                new Argument<string>("file")
            };
            hashObjectCmd.Handler = CommandHandler.Create<string>(HashObject);

            var catFileCmd = new Command("cat-file", "look up a file by object id")
            {
                new Argument<string>("object")
            };
            catFileCmd.Handler = CommandHandler.Create<string>(CatFile);

            var readTreeCmd = new Command("read-tree", "read out tree")
            {
                new Argument<string>("tree")
            };
            readTreeCmd.Handler = CommandHandler.Create<string>(ReadTree);

            var commitCmd = new Command("commit")
            {
                new Option<string>(new string[]{"-m", "--message"}, "message")
            };
            commitCmd.Handler = CommandHandler.Create<string>(Commit);

            var logCmd = new Command("log")
            {
                new Argument<string>("oid", () => "@"),
            };
            logCmd.Handler = CommandHandler.Create<string>(Log);

            var checkoutCmd = new Command("checkout")
            {
                new Argument<string>("commit")
            };
            checkoutCmd.Handler = CommandHandler.Create<string>(Checkout);

            var tagCmd = new Command("tag")
            {
                new Argument<string>("name", ()=>string.Empty),
                new Argument<string>("oid", () => "@")
            };
            tagCmd.Handler = CommandHandler.Create<string, string>(TagOp);

            var branchCmd = new Command("branch")
            {
                new Argument<string>("name", () => string.Empty),
                new Argument<string>("oid", () => "@")
            };
            branchCmd.Handler = CommandHandler.Create<string, string>(Branch);

            var statusCmd = new Command("status");
            statusCmd.Handler = CommandHandler.Create(Status);

            var resetCmd = new Command("reset")
            {
                new Argument<string>("commit")
            };
            resetCmd.Handler = CommandHandler.Create<string>(Reset);

            var showCmd = new Command("show")
            {
                new Argument<string>("oid", () => "@")
            };
            showCmd.Handler = CommandHandler.Create<string>(Show);
            
            var diffCmd = new Command("diff")
            {
                new Argument<string>("commit", () => "@")
            };
            diffCmd.Handler = CommandHandler.Create<string>(Different);
            
            var mergeCmd = new Command("merge")
            {
                new Argument<string>("commit", () => "@")
            };
            mergeCmd.Handler = CommandHandler.Create<string>(Merge);

            var addCmd = new Command("add")
            {
                new Argument<IEnumerable<string>>("files"),
            };
            addCmd.Handler = CommandHandler.Create<IEnumerable<string>>(Add);

            var fetchCmd = new Command("fetch")
            {
                new Argument<string>("remote")
            };
            fetchCmd.Handler = CommandHandler.Create<string>(Fetch);

            var pushCmd = new Command("push")
            {
                new Argument<string>("remote"),
                new Argument<string>("branch")
            };
            pushCmd.Handler = CommandHandler.Create<string,string>(Push);
            
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
            };

            return rootCommand;
        }

        private static void Push(string remote, string branch)
        {
            IDataProvider remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), remote);
            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                DataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation);
            string refName = Path.Join("refs", "heads", branch);
            remoteOperation.Push(refName);
        }

        private static void Fetch(string remote)
        {
            IDataProvider remoteDataProvider = new LocalDataProvider(new PhysicalFileOperator(new FileSystem()), remote);
            ICommitOperation remoteCommitOperation = new DefaultCommitOperation(remoteDataProvider, new DefaultTreeOperation(remoteDataProvider));

            IRemoteOperation remoteOperation = new DefaultRemoteOperation(
                DataProvider,
                CommitOperation,
                remoteDataProvider,
                remoteCommitOperation);
            remoteOperation.Fetch();
        }

       
        private static void Add(IEnumerable<string> files)
        {
            AddOperation.Add(files);
        }

        private static void Merge(string commit)
        {
            commit = OidConverter(commit);
            MergeOperation.Merge(commit);
        }
        
        private static void Different(string commit)
        {
            commit = OidConverter(commit);
            var tree = CommitOperation.GetCommit(commit).Tree;
            var result = Diff.DiffTrees(TreeOperation.GetTree(tree), TreeOperation.GetWorkingTree());
            Console.WriteLine(result);
        }

        private static void Show(string oid)
        {
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
            Console.WriteLine($"commit {oid}{refStr}\n");
            Console.WriteLine($"{commit.Message}     ");
            Console.WriteLine(string.Empty);
        }

        private static void Reset(string commit)
        {
            string oid = OidConverter(commit);
            ResetOperation.Reset(oid);
        }
        
        private static void Status()
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

        static void TagOp(string name, string oid)
        {
            
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
        
        private static void Checkout(string commit)
        {
            CheckoutOperation.Checkout(commit);

        }
        

        private static void Init()
        {
            InitOperation.Init();
            Console.WriteLine($"Initialized empty ugit repository in {DataProvider.GitDirFullPath}");
        }

        private static void HashObject(string file)
        {
            byte[] data = FileSystem.File.ReadAllBytes(file);
            Console.WriteLine(DataProvider.WriteObject(data));
        }

        private static void CatFile(string oid)
        {
            byte[] data = DataProvider.GetObject(OidConverter(oid));
            if (data.Length > 0)
            {
                Console.WriteLine(data.Decode());
            }
        }

        private static void ReadTree(string tree)
        {
            TreeOperation.ReadTree(OidConverter(tree));
        }

        private static void Commit(string message)
        {
            try
            {
                Console.WriteLine(CommitOperation.CreateCommit(message));
            }
            catch (UgitException e)
            {
                Console.WriteLine(e.Message);
            }
        }
        

        static void Log(string oid)
        {
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
        
        private static void Branch(string name, string oid)
        {
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
    }
}
