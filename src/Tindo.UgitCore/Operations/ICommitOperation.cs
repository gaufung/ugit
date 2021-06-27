using System.Collections.Generic;

namespace Tindo.UgitCore.Operations
{
    public interface ICommitOperation
    {
        Commit Get(string oid);

        string Create(string message);

        IEnumerable<string> GetHistory(IEnumerable<string> oids);

        IEnumerable<string> GetObjectsFromHistory(IEnumerable<string> oids);
    }
}
