namespace Ugit.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface of add operations.
    /// </summary>
    internal interface IAddOperation
    {
        /// <summary>
        /// Add files.
        /// </summary>
        /// <param name="fileNames">file names.</param>
        void Add(IEnumerable<string> fileNames);
    }
}
