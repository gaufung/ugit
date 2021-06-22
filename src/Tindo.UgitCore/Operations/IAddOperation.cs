using System.Collections.Generic;

namespace Tindo.UgitCore.Operations
{
    public interface IAddOperation
    {
        void Add(IEnumerable<string> fileNames);
    }
}