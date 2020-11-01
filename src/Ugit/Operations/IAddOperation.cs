namespace Ugit.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface of add operations.
    /// </summary>
    internal interface IAddOperation
    {
        /// <summary>
        /// Add file to stage index.
        /// </summary>
        /// <param name="index">index.</param>
        /// <param name="fileName">file name.</param>
        void AddFile(IDictionary<string, string> index, string fileName);

        /// <summary>
        /// Add directory to stage index.
        /// </summary>
        /// <param name="index">the index.</param>
        /// <param name="directoryName">The directory name.</param>
        void AddDirectionary(IDictionary<string, string> index, string directoryName);
    }
}
