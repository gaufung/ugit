namespace Ugit.Operations
{
    using System.IO;

    /// <summary>
    /// Default implementation of <see cref="IInitOperation"/>.
    /// </summary>
    internal class DefaultInitOperation : IInitOperation
    {
        private readonly IDataProvider dataProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultInitOperation"/> class.
        /// </summary>
        /// <param name="dataProvider">Data provider.</param>
        public DefaultInitOperation(IDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;
        }

        /// <inheritdoc/>
        public void Init()
        {
            this.dataProvider.Init();
            this.dataProvider.UpdateRef("HEAD", RefValue.Create(true, Path.Join("refs", "heads", "master")));
        }
    }
}
