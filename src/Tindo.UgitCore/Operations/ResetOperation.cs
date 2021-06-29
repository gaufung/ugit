using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore.Operations
{
    public class ResetOperation : IResetOperation
    {
        private readonly IDataOperator dataOperator;

        private readonly ILogger<ResetOperation> logger;

        public ResetOperation(IDataOperator dataOperator, ILoggerFactory loggerFactory)
        {
            this.dataOperator = dataOperator;
            this.logger = loggerFactory.CreateLogger<ResetOperation>();
        }

        public void Reset(string oid)
        {
            this.dataOperator.UpdateRef(Constants.HEAD, RefValue.Create(false, oid));
        }
    }
}
