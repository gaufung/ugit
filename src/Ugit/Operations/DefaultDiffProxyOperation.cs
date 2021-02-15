namespace Ugit.Operations
{
    using System.Diagnostics;

    /// <summary>
    /// The implementation of <see cref="IDiffOperation"/>.
    /// </summary>
    public class DefaultDiffProxyOperation : IDiffProxyOperation
    {
        /// <inheritdoc/>
        public (int, string, string) Execute(string name, string arguments)
        {
#if NET5_0
            Process process = new ();
#else
            Process process = new Process();
#endif
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
