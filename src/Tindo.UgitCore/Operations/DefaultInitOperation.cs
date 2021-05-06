namespace Tindo.UgitCore.Operations
{
    using System.IO;
    
    public class DefaultInitOperation : IInitOperation
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
            this.dataProvider.UpdateRef(Constants.HEAD, RefValue.Create(true, Path.Join(Constants.Refs, Constants.Heads, Constants.Master)));
        }
    }
}