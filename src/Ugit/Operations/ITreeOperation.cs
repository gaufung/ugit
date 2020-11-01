﻿namespace Ugit.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// Tree related operations.
    /// </summary>
    internal interface ITreeOperation
    {
        /// <summary>
        /// Get the tree from the tree object id.
        /// </summary>
        /// <param name="oid">Tree object id.</param>
        /// <param name="basePath">The base path.</param>
        /// <returns>The tree. {filePath : object id}.</returns>
        IDictionary<string, string> GetTree(string oid, string basePath = ".");

        /// <summary>
        /// Read the tree to index.
        /// </summary>
        /// <param name="treeOid">The object id.</param>
        /// <param name="updateWorking">Whether to update the working directory.</param>
        void ReadTree(string treeOid, bool updateWorking = false);

        /// <summary>
        /// Write tree from index to repo.
        /// </summary>
        /// <returns>The return object id.</returns>
        string WriteTree();

        /// <summary>
        /// Write index to the current working.
        /// </summary>
        /// <param name="index">The index value</param>
        void CheckoutIndex(Dictionary<string, string> index);
    }
}
