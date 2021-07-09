namespace Tindo.Ugit
{
    using System.Diagnostics;

    /// <summary>
    /// The implementation of <see cref="IDiffOperation"/>.
    /// </summary>
    public class DiffProxy : IDiffProxy
    {
        /// <inheritdoc/>
        public (int, string, string) Execute(string name, string arguments)
        {
            Process process = new Process
            {
                StartInfo =
                {
                    FileName = name,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                },
            };
            process.Start();
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();
            return (process.ExitCode, output, error);
        }
    }
}
