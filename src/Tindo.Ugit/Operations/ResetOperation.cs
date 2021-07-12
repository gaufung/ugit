
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
        {
            this.dataProvider = dataProvider;
            this.logger = new NullLogger<ResetOperation>();
        }

        public ResetOperation(IDataProvider dataProvider, ILoggerFactory loggerFactory)
            : this(dataProvider)
        {
            this.logger = loggerFactory.CreateLogger<ResetOperation>();
        }

        /// <inheritdoc/>
        public void Reset(string oid)
        {
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
        }
    }
}
