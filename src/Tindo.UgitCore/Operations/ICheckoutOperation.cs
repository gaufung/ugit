namespace Tindo.UgitCore.Operations
{
    /// <summary>
    /// Checkout operation.
    /// </summary>
    public interface ICheckoutOperation
    {
        /// <summary>
        /// Checkout from name.
        /// </summary>
        /// <param name="name">The name to checkout.</param>
        void Checkout(string name);
    }
}