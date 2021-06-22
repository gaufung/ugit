using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Logging;

namespace Tindo.UgitCore.Operations
{
    public class BranchOperation : IBranchOperation
    {
        private readonly IDataOperator _dataOperator;

        private readonly ILogger<BranchOperation> _logger;

        public BranchOperation(IDataOperator dataOperator, ILoggerFactory loggerFactory)
        {
            this._dataOperator = dataOperator;
            this._logger = loggerFactory.CreateLogger<BranchOperation>();
        }

        public IEnumerable<string> Names
        {
            get
            {
                string @ref = Path.Join(Constants.Refs, Constants.Heads);
                
                foreach (var (refName,_) in this._dataOperator.GetAllRefs(@ref))
                {
                    _logger.LogInformation($"getting a ref {refName}");
                    yield return Path.GetRelativePath(Path.Join(Constants.Refs, Constants.Heads), 
                        refName);
                }
            }
        }

        public string Current
        {
            get
            {
                var HEAD = this._dataOperator.GetRef(Constants.HEAD, false);
                if (!HEAD.Symbolic)
                {
                    return null;
                }

                var head = HEAD.Value;
                this._logger.LogInformation($"head is point to {head}");
                if (!head.StartsWith(Path.Join(Constants.Refs, Constants.Heads)))
                {
                    throw new UgitException("Branch ref should start with refs/heads");
                }

                return Path.GetRelativePath(Path.Join(Constants.Refs, Constants.Heads), head);
            }
        }
        public void Create(string name, string oid)
        {
            string @ref = Path.Join(Constants.Refs, Constants.Heads, name);
            this._dataOperator.UpdateRef(@ref, RefValue.Create(false,oid));
        }

        public bool IsBranch(string name)
        {
            string filePath = Path.Join(Constants.Refs, Constants.Heads, name);
            return !string.IsNullOrWhiteSpace(this._dataOperator.GetRef(filePath).Value);
        }
    }
}