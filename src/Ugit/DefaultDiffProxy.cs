using System.Diagnostics;

namespace Ugit
{
    public class DefaultDiffProxy : IDiffProxy
    {
        public (int, string, string) Execute(string name, string arguments)
        {
            Process process = new Process();
            process.StartInfo.FileName = name;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output, error);
        }
    }
}
