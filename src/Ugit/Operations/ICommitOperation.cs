namespace Ugit.Operations
{
    using System.Collections.Generic;

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

        /// <summary>
        /// Iter commits and parents.
        /// </summary>
        /// <param name="oids">oids.</param>
        /// <returns>histrory oids.</returns>
        IEnumerable<string> GetCommitHistory(IEnumerable<string> oids);
    }
}
