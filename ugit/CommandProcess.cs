using System;
using System.Diagnostics;

namespace ugit
{
    public class CommandProcess: ICommandProcess
    {
        public ValueTuple<int, string, string> Execute(string command, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = command;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return ValueTuple.Create(process.ExitCode, output, error);
        }
        
    }
}