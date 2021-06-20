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

            var rootcmd = new RootCommand
            {
                initCmd,
                new Command("status", "check repository status")
                {
                    Handler = CommandHandler.Create(Status)
                },
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

        static void Status()
        {
            Console.WriteLine("repository status");
        }
    }
}
