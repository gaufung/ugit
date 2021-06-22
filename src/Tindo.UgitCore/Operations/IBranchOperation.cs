using System.Collections.Generic;

namespace Tindo.UgitCore.Operations
{
    public interface IBranchOperation
    {
        IEnumerable<string> Names { get; }
        
        string Current { get; }

        void Create(string name, string oid);

        bool IsBranch(string name);
    }
}