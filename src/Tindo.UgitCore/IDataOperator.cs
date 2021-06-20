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
    }
}
