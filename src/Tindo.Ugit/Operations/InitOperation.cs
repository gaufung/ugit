namespace Tindo.Ugit
{
    using System.IO;

    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Default implementation of <see cref="IInitOperation"/>.
    /// </summary>
    internal class InitOperation : IInitOperation
    {
        private readonly IDataProvider dataProvider;

        private readonly ILogger<InitOperation> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="InitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">Data provider.</param>
        public InitOperation(IDataProvider dataProvider, ILogger<InitOperation> logger)
        {
            this.dataProvider = dataProvider;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public void Init()
        {
            this.dataProvider.Init();
            this.logger.LogInformation("Initilize an empty repository");
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(true, Path.Join(Constants.Refs, Constants.Heads, Constants.Master)));
            this.logger.LogInformation("Create master branch");
        }
    }
}
