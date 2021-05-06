namespace Tindo.UgitCore.Operations
{
    public interface IResetOperation
    {
        /// <summary>
        /// Reset to object id.
        /// </summary>
        /// <param name="oid">The object id.</param>
        void Reset(string oid);
    }
}