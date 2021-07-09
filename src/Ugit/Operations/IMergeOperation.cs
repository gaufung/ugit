namespace Tindo.Ugit
{
    /// <summary>
    /// Interface for merge operation.
    /// </summary>
    internal interface IMergeOperation
    {
        /// <summary>
        /// Merge with other commit.
        /// </summary>
        /// <param name="other">The other commit.</param>
        void Merge(string other);
    }
}
