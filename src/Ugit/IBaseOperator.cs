namespace Ugit
{
    internal interface IBaseOperator
    {
        string WriteTree(string directory = ".");

        void ReadTree(string treeOid);

        string Commit(string message);

        Commit GetCommit(string oid);

        void Checkout(string oid);
    }
}
