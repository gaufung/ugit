
namespace Tindo.Ugit
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;

    /// <summary>
    /// Default reset operation.
    /// </summary>
    internal class ResetOperation : IResetOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ILogger<ResetOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        public ResetOperation(IDataProvider dataProvider)
            : this(dataProvider, NullLogger<ResetOperation>.Instance)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResetOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="logger">The logger.</param>
        public ResetOperation(IDataProvider dataProvider, ILogger<ResetOperation> logger)
        {
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Reset(string oid)
        {
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
        }
    }
}
