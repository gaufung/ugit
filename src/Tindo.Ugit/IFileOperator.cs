namespace Tindo.Ugit
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Subset operation of file operator no matter of local or remote file system.
    /// </summary>
    internal interface IFileOperator
    {
        /// <summary>
        /// Gets Current directory.
        /// </summary>
        string CurrentDirectory { get; }

        /// <summary>
        /// Whether file or directory exist.
        /// </summary>
        /// <param name="path">the Path.</param>
        /// <param name="isFile">whether is file or directory.</param>
        /// <returns>True if exist.</returns>
        bool Exists(string path, bool isFile = true);

        /// <summary>
        /// Write a byte array to a file path.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <param name="bytes">the byte array.</param>
        void Write(string path, byte[] bytes);

        /// <summary>
        /// Read bytes from given file.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <param name="bytes">byte read out.</param>
        /// <returns>byte array that file contains.</returns>
        bool TryRead(string path, out byte[] bytes);

        /// <summary>
        /// Read the bytes from file path. Throw <see cref="UgitException"/> if doesn't exist.
        /// </summary>
        /// <param name="path">file path.</param>
        /// <returns>bytes read out.</returns>
        byte[] Read(string path);

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path">filePath.</param>
        /// <param name="isFile">if file or directory.</param>
        void Delete(string path, bool isFile = true);

        /// <summary>
        /// Walk in given directory path.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <returns>The file path list.</returns>
        IEnumerable<string> Walk(string path);

        /// <summary>
        /// Empty current directory.
        /// </summary>
        /// <param name="ignore">which file to ignore.</param>
        void EmptyCurrentDirectory(Func<string, bool> ignore);

        /// <summary>
        /// Create a directory.
        /// </summary>
        /// <param name="directory">directory to create.</param>
        /// <param name="force">force: delete if existing.</param>
        void CreateDirectory(string directory, bool force = true);
    }
}
