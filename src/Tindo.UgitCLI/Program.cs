namespace Tindo.UgitCLI
{
    using System;
    using System.CommandLine;
    using System.CommandLine.Invocation;

    class Program
    {
        static void Main(string[] args)
        {
            var cmd = new RootCommand
            {
                new Command("init", "initialize ugit repository")
                {
                    Handler = CommandHandler.Create(Initialize)
                },
                new Command("status", "check repository status")
                {
                    Handler = CommandHandler.Create(Status)
                },
            };

            cmd.Invoke(args);
        }

        static void Initialize()
        {
            Console.WriteLine("Initialize ugit repository successfully");
        }

        static void Status()
        {
            Console.WriteLine("repository status");
        }
    }
}
