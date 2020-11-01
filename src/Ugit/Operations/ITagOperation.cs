namespace Ugit.Operations
{
    /// <summary>
    /// Tag operation interface.
    /// </summary>
    internal interface ITagOperation
    {
        /// <summary>
        /// Create an tag.
        /// </summary>
        /// <param name="name">tag name.</param>
        /// <param name="oid">tag oid.</param>
        void Create(string name, string oid);
    }
}
