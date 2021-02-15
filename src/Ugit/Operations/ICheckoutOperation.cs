namespace Ugit.Operations
{
    /// <summary>
    /// Checkout operation.
    /// </summary>
    internal interface ICheckoutOperation
    {
        /// <summary>
        /// Checkout from name.
        /// </summary>
        /// <param name="name">The name to checkout.</param>
        void Checkout(string name);
    }
}
