namespace Ugit
{
    /// <summary>
    /// The Diff command proxy.
    /// </summary>
    internal interface IDiffProxy
    {
        /// <summary>
        /// Execute diff command.
        /// </summary>
        /// <param name="name">the command name.</param>
        /// <param name="arguments">the aruguments.</param>
        /// <returns>the result. {exit code, output, error output}</returns>
        (int, string, string) Execute(string name, string arguments);
    }
}
