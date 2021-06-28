using System.Collections.Generic;

namespace Tindo.UgitCore.Operations
{
    public interface ITagOperation
    {
        IEnumerable<string> All { get; }

        void Create(string name, string oid);
    }
}
