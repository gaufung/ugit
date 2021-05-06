namespace Tindo.UgitCore.Operations
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface of add operations.
    /// </summary>
    public interface IAddOperation
    {
        /// <summary>
        /// Add files.
        /// </summary>
        /// <param name="fileNames">file names.</param>
        void Add(IEnumerable<string> fileNames);
    }
}