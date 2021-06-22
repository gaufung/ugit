using System.Collections.Generic;

namespace Tindo.UgitCLI
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.DependencyInjection;
    using Tindo.UgitCore.Operations;
    using Tindo.UgitCore;
    using System.IO.Abstractions;

    class Program
    {
        
        static void Main(string[] args)
        {
            CreateArgsCommand().Invoke(args);
        }

        static Command CreateArgsCommand()
        {
            var initCmd = new Command("init", "initialize ugit repository")
            {
                new Option(new string[] {"--verbose", "-v" }, "Show ugit operation" )
            };
            initCmd.Handler = CommandHandler.Create<bool>(Initialize);

            var statusCmd = new Command("status", "show ugit repository status")
            {
                new Option(new string[] {"--verbose", "-v" }, "Show ugit operation" )
            };
            statusCmd.Handler = CommandHandler.Create<bool>(Status);
            
            var branchCmd = new Command("branch", "show branch status or create a branch")
            {
                new Argument<string>("name", ()=> string.Empty),
                new Argument<string>("startPoint", ()=> string.Empty),
                new Option(new string[] {"--verbose", "-v" }, "Show ugit operation" )
            };
            branchCmd.Handler = CommandHandler.Create<string, string, bool>(Branch);

            var addCmd = new Command("add", "add files to the index")
            {
                new Argument<IEnumerable<string>>("files"),
                new Option(new string[] {"--verbose", "-v" }, "Show ugit operation" )
            };
            
            addCmd.Handler =CommandHandler.Create<IEnumerable<string>, bool>(Add);
            
            var rootcmd = new RootCommand
            {
                initCmd,
                statusCmd,
                branchCmd,
                addCmd,
            };

            return rootcmd;
        }

        static ILoggerFactory CreateLoggerFactory(LogLevel minimumLevel)
        {
            return new ServiceCollection()
                .AddLogging(builder =>
                        builder.AddConsole()
                        )
                .Configure<LoggerFilterOptions>(option =>
                {
                    option.MinLevel = minimumLevel;
                })
                .BuildServiceProvider()
                .GetRequiredService<ILoggerFactory>();
        }

        static void Initialize(bool verbose)
        {
            IFileOperator fileOperator = new PhysicalFileOperator(new FileSystem());
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose ? LogLevel.Information : LogLevel.Error);
            IDataOperator dataOperator = new LocalDataOperator(fileOperator, loggerFactory);
            IInitOperation operation = new InitOpeartion(dataOperator, loggerFactory);
            operation.Initialize();
        }

        static void Status(bool verbose)
        {
            IFileOperator fileOperator = new PhysicalFileOperator(new FileSystem());
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose ? LogLevel.Information : LogLevel.Error);
            IDataOperator dataOperator = new LocalDataOperator(fileOperator, loggerFactory);
            
            IBranchOperation branchOperation = new BranchOperation(dataOperator, loggerFactory);
            string branch = branchOperation.Current;
            if (!string.IsNullOrEmpty(branch))
            {
                Console.WriteLine($"On branch {branch}");
            }
        }

        static void Branch(string name, string startPoint, bool verbose)
        {
            IFileOperator fileOperator = new PhysicalFileOperator(new FileSystem());
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose ? LogLevel.Information : LogLevel.Error);
            IDataOperator dataOperator = new LocalDataOperator(fileOperator, loggerFactory);

            startPoint = dataOperator.GetOid(startPoint);

            var branchOperation = new BranchOperation(dataOperator, loggerFactory);
            if (string.IsNullOrWhiteSpace(name))
            {
                string current = branchOperation.Current;
                foreach (var branch in branchOperation.Names)
                {
                    string prefix = branch == current ? "*" : string.Empty;
                    Console.WriteLine($"{prefix}{branch}");
                }
            }
            else if (!string.IsNullOrEmpty(startPoint))
            {
                branchOperation.Create(name, startPoint);
                Console.WriteLine($"Branch {name} created at {startPoint.Substring(0, 10)}");
            }
        }

        static void Add(IEnumerable<string> files,bool verbose)
        {
            IFileOperator fileOperator = new PhysicalFileOperator(new FileSystem());
            ILoggerFactory loggerFactory = CreateLoggerFactory(verbose ? LogLevel.Information : LogLevel.Error);
            IDataOperator dataOperator = new LocalDataOperator(fileOperator, loggerFactory);

            IAddOperation addOperation = new AddOperation(dataOperator, fileOperator, loggerFactory);
            addOperation.Add(files);
        }
    }
}
