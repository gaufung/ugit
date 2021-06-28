using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;
namespace Tindo.UgitCore.Operations
{
    public class TagOperation : ITagOperation
    {
        private readonly IDataOperator dataOperator;

        private readonly ILogger<TagOperation> logger;

        public TagOperation(IDataOperator dataOperator, ILoggerFactory loggerFactory)
        {
            this.dataOperator = dataOperator;
            this.logger = loggerFactory.CreateLogger<TagOperation>();

        }

        public IEnumerable<string> All 
        {
            get
            {
                string prefix = Path.Join(Constants.Refs, Constants.Tags);
                foreach (var (tagRef, _) in this.dataOperator.GetAllRefs(prefix, false))
                {
                    yield return Path.GetRelativePath(prefix, tagRef);
                }
            }
        }

        public void Create(string name, string oid)
        {
            string @ref = Path.Join(Constants.Refs, Constants.Tags, name);
            this.dataOperator.UpdateRef(@ref, RefValue.Create(false, oid));
        }
    }
}
