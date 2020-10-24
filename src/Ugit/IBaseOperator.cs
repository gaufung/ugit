using System.Collections.Generic;
namespace Ugit
{
    internal interface IBaseOperator
    {
        string WriteTree(string directory = ".");

        void ReadTree(string treeOid);

        string Commit(string message);

        Commit GetCommit(string oid);

        void Checkout(string oid);

        void CreateTag(string name, string oid);

        string GetOid(string name);

        IEnumerable<string> IterCommitsAndParents(IEnumerable<string> oids);

        void CreateBranch(string name, string oid);

        void Init();

        string GetBranchName();

        IEnumerable<string> IterBranchNames();

        void Reset(string oid);

        IDictionary<string, string> GetTree(string treeOid, string basePath = "");
    }
}
