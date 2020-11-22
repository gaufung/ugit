namespace Ugit
{
    /// <summary>
    /// Remote operation interface.
    /// </summary>
    internal interface IRemoteOperation
    {
        /// <summary>
        /// Fetch the everything from the remote.
        /// </summary>
        void Fetch();

        /// <summary>
        /// Push ref to the remote.
        /// </summary>
        /// <param name="refName">the ref name.</param>
        void Push(string refName);
    }
}
