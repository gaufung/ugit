namespace Ugit.Operations
{
    /// <summary>
    /// Reset operation interface.
    /// </summary>
    internal interface IResetOperation
    {
        /// <summary>
        /// Reset to object id.
        /// </summary>
        /// <param name="oid">The object id.</param>
        void Reset(string oid);
    }
}
