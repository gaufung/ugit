namespace Tindo.UgitCore
{
    using System.Collections.Generic;

    /// <summary>
    /// Data provider for file operation.
    /// </summary>
    public interface IDataProvider : IFileOperator
    {
        /// <summary>
        /// Gets the git directory full path.
        /// </summary>
        string GitDirFullPath { get; }

        /// <summary>
        /// Gets the git directory name.
        /// </summary>
        string GitDir { get;  }

        /// <summary>
        /// Gets or sets Index.
        /// </summary>
        Dictionary<string, string> Index { get; set; }

        /// <summary>
        /// Initialize the git directory.
        /// </summary>
        void Init();

        /// <summary>
        /// Hash an object.
        /// </summary>
        /// <param name="data">The object byte array.</param>
        /// <param name="type">The object type.</param>
        /// <returns>The object id digest.</returns>
        string HashObject(byte[] data, string type = "blob");

        /// <summary>
        /// Get the object by object id.
        /// </summary>
        /// <param name="oid">The object id.</param>
        /// <param name="expected">The expect blob type.</param>
        /// <returns>The blob file data.</returns>
        byte[] GetObject(string oid, string expected = "blob");

        /// <summary>
        /// Update the reference.
        /// </summary>
        /// <param name="ref">The reference.</param>
        /// <param name="value">The reference value.</param>
        /// <param name="deref">Whether need to de-reference for a reference.</param>
        void UpdateRef(string @ref, RefValue value, bool deref = true);

        /// <summary>
        /// Get reference.
        /// </summary>
        /// <param name="ref">The reference value.</param>
        /// <param name="deref">Whether need to de-reference.</param>
        /// <returns>The ref value.</returns>
        RefValue GetRef(string @ref, bool deref = true);

        /// <summary>
        /// Iterate reference.
        /// </summary>
        /// <param name="prefix">The refer prefix.</param>
        /// <param name="deref">Whether needs to de-reference.</param>
        /// <returns>The reference list.</returns>
        IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true);

        /// <summary>
        /// Delete reference.
        /// </summary>
        /// <param name="ref">The reference name.</param>
        /// <param name="deref">Whether needs to deference.</param>
        void DeleteRef(string @ref, bool deref = true);

        /// <summary>
        /// Get object id from name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The object id.</returns>
        string GetOid(string name);

        /// <summary>
        /// Should ignore this file or directory. (.ugit) folder.
        /// </summary>
        /// <param name="path">the file path or directory.</param>
        /// <returns>True if it need to be ignored.</returns>
        bool IsIgnore(string path);

        /// <summary>
        /// Object exists by given object or not.
        /// </summary>
        /// <param name="oid">object Id.</param>
        /// <returns>True if existing.</returns>
        bool ObjectExist(string oid);
    }
}