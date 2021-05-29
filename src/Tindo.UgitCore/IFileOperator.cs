namespace Tindo.UgitCore
{
    using System.Collections.Generic;
    using System;

    /// <summary>
    /// Phsical file operator
    /// </summary>
    public interface IFileOperator
    {
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
        /// <returns>byte array that file contains.</returns>
        byte[] Read(string path);

        /// <summary>
        /// Delete a file.
        /// </summary>
        /// <param name="path">filePath.</param>
        void Delete(string path);

        /// <summary>
        /// Walk in given directory path.
        /// </summary>
        /// <param name="path">the file path.</param>
        /// <returns>The file path list.</returns>
        IEnumerable<string> Walk(string path);

        /// <summary>
        /// Empty repo directory.
        /// </summary>
        void EmptyCurrentDirectory(Func<string, bool> ignore);

        /// <summary>
        /// Try to read the bytes from a file.
        /// </summary>
        /// <param name="path">Read a file path.</param>
        /// <param name="bytes">Read the value</param>
        /// <returns>true if read success.</returns>
        bool TryRead(string path, out byte[] bytes);

        void CreateDirectory(string directory);

        string CurrentDirectory { get;  }
    }
}