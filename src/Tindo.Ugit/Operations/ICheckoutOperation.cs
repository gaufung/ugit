namespace Tindo.Ugit
{
    /// <summary>
    /// Checkout opeartion.
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
