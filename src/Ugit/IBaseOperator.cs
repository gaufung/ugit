using System.Collections.Generic;
namespace Ugit
{
    internal interface IBaseOperator
    {
        string WriteTree();

        void ReadTree(string treeOid, bool updateWorking = false);

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

        IDictionary<string, string> GetWorkingTree();

        void Merge(string other);

        string GetMergeBase(string oid1, string oid2);

        void Add(IEnumerable<string> fileNames);

        Dictionary<string, string> GetIndexTree();

    }
}
