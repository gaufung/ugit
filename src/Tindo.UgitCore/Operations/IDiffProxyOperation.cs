namespace Tindo.UgitCore.Operations
{
    /// <summary>
    /// The Diff command proxy.
    /// </summary>
    public interface IDiffProxyOperation
    {
        /// <summary>
        /// Execute diff command.
        /// </summary>
        /// <param name="name">the command name.</param>
        /// <param name="arguments">the arguments.</param>
        /// <returns>the result. {exit code, output, error output}.</returns>
        (int, string, string) Execute(string name, string arguments);
    }
}