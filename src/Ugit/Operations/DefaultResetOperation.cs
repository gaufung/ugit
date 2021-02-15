namespace Ugit.Operations
{
    /// <summary>
    /// Default reset operation.
    /// </summary>
    internal class DefaultResetOperation : IResetOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultResetOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public DefaultResetOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public void Reset(string oid)
        {
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
        }
    }
}
