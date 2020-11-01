namespace Ugit.Operations
{
    /// <summary>
    /// Commit Operation interface.
    /// </summary>
    internal interface ICommitOperation
    {
        /// <summary>
        /// Get commit according to the object id.
        /// </summary>
        /// <param name="oid">The object id.</param>
        /// <returns>The commit object.</returns>
        Commit GetCommit(string oid);

        /// <summary>
        /// Create commit with commit message.
        /// </summary>
        /// <param name="message">The commit message.</param>
        /// <returns>Create Commit.</returns>
        string CreateCommit(string message);
    }
}
