namespace Ugit
{
    using System.Collections.Generic;

    /// <summary>
    /// Subset operation of file operator.
    /// </summary>
    internal interface IFileOperator
    {
        /// <summary>
        /// Whether file or directory exist.
        /// </summary>
        /// <param name="path">the Path.</param>
        /// <param name="isFile">whether is file or directory.</param>
        /// <returns>True if exist.</returns>
        bool Exist(string path, bool isFile = true);

        /// <summary>
        /// Write a byte array to a file path.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <param name="bytes">the byte array.</param>
        void WriteAllBytes(string path, byte[] bytes);

        /// <summary>
        /// Read bytes from given file.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <returns>byte array that file contains.</returns>
        byte[] ReadAllBytes(string path);

        /// <summary>
        /// Walk in given directory path.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <returns>The file path list.</returns>
        IEnumerable<string> Walk(string path);

        /// <summary>
        /// Empty current directory.
        /// </summary>
        void EmptyCurrentDirectory();

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path">filePath.</param>
        void Delete(string path);
    }
}
