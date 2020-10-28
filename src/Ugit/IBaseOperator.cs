namespace Ugit
{
    using System.Collections.Generic;

    /// <summary>
    /// IBase operator.
    /// </summary>
    internal interface IBaseOperator
    {
        /// <summary>
        /// Write a tree.
        /// </summary>
        /// <returns>The tree object id.</returns>
        string WriteTree();

        /// <summary>
        /// Read a tree.
        /// </summary>
        /// <param name="treeOid">the tree object id.</param>
        /// <param name="updateWorking">need to update working directory.</param>
        void ReadTree(string treeOid, bool updateWorking = false);

        /// <summary>
        /// Make a commit.
        /// </summary>
        /// <param name="message">the commit message.</param>
        /// <returns>The commit oid.</returns>
        string Commit(string message);

        /// <summary>
        /// Get the commit acccording to the object id.
        /// </summary>
        /// <param name="oid">the commit oid.</param>
        /// <returns>The commit.</returns>
        Commit GetCommit(string oid);

        /// <summary>
        /// Check out.
        /// </summary>
        /// <param name="oid">The checkout object id.</param>
        void Checkout(string oid);

        /// <summary>
        /// Create tag.
        /// </summary>
        /// <param name="name">the tag name.</param>
        /// <param name="oid">The tag object id.</param>
        void CreateTag(string name, string oid);

        /// <summary>
        /// Get oid according to the name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>the oid.</returns>
        string GetOid(string name);

        /// <summary>
        /// Iter commits and parents.
        /// </summary>
        /// <param name="oids">the oids. </param>
        /// <returns>the ancestor commit.</returns>
        IEnumerable<string> IterCommitsAndParents(IEnumerable<string> oids);

        /// <summary>
        /// Create branch from oid.
        /// </summary>
        /// <param name="name">The branch name.</param>
        /// <param name="oid">The object id.</param>
        void CreateBranch(string name, string oid);

        /// <summary>
        /// Init operation.
        /// </summary>
        void Init();

        /// <summary>
        /// Get current branch name.
        /// </summary>
        /// <returns>Current branch name.</returns>
        string GetBranchName();

        /// <summary>
        /// Iteratre names names.
        /// </summary>
        /// <returns>The branch name.</returns>
        IEnumerable<string> IterBranchNames();

        /// <summary>
        /// Reset opeartion.
        /// </summary>
        /// <param name="oid">The object id.</param>
        void Reset(string oid);

        /// <summary>
        /// Get tree based on the tree oid.
        /// </summary>
        /// <param name="treeOid">The tree object id.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns>The tree id.{path: oid}.</returns>
        IDictionary<string, string> GetTree(string treeOid, string basePath = "");

        /// <summary>
        /// Get working tree.
        /// </summary>
        /// <returns>The current working tree. {path: oid}.</returns>
        IDictionary<string, string> GetWorkingTree();

        /// <summary>
        /// Merge with other oid.
        /// </summary>
        /// <param name="other">other object id.</param>
        void Merge(string other);

        /// <summary>
        /// Get common ancestor object id.
        /// </summary>
        /// <param name="oid1">Object id #1. </param>
        /// <param name="oid2">Object id #2.</param>
        /// <returns>The ancestor object id.</returns>
        string GetMergeBase(string oid1, string oid2);

        /// <summary>
        /// Add files or directories.
        /// </summary>
        /// <param name="fileNames">File name or directory.</param>
        void Add(IEnumerable<string> fileNames);

        /// <summary>
        /// Set stage index tree.
        /// </summary>
        /// <returns>The index tree.</returns>
        Dictionary<string, string> GetIndexTree();
    }
}
