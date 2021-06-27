using System.Collections.Generic;


namespace Tindo.UgitCore
{
    public interface IDataOperator
    {
        byte[] GetObject(string oid, string expected = "blob");

        string WriteObject(byte[] data, string type = "blob");

        void Initialize();

        string RepositoryPath { get; }

        RefValue GetRef(string @ref, bool deref = true);

        void UpdateRef(string @ref, RefValue value, bool deref = true);

        void DeleteRef(string @ref, bool deref = true);

        IEnumerable<(string, RefValue)> GetAllRefs(string prefix = "", bool deref = true);

        string GetOid(string name);
        
        Tree Index { get; set; }

        Configuration Config { get; set; }
    }
}
