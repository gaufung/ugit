namespace Tindo.UgitCore.Operations
{
    /// <summary>
    /// Interface for merge operation.
    /// </summary>
    public interface IMergeOperation
    {
        /// <summary>
        /// Merge with other commit.
        /// </summary>
        /// <param name="other">The other commit.</param>
        void Merge(string other);
    }
}