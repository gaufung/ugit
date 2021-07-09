namespace Tindo.Ugit
{
    /// <summary>
    /// Default reset operation.
    /// </summary>
    internal class ResetOperation : IResetOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public ResetOperation(IDataProvider dataProvider)
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
