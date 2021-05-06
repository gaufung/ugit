namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// The branch opeartion.
    /// </summary>
    public interface IBranchOperation
    {
        /// <summary>
        /// Gets branch names.
        /// </summary>
        IEnumerable<string> Names { get; }

        /// <summary>
        /// Gets current name.
        /// </summary>
        string Current { get; }

        /// <summary>
        /// Create a branch.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="oid">The branch oid.</param>
        void Create(string name, string oid);

        /// <summary>
        /// Whether name is branch.
        /// </summary>
        /// <param name="branch">branch name.</param>
        /// <returns>True if branch.</returns>
        bool IsBranch(string branch);
    }
}