using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tindo.UgitCore.Operations
{
    public interface ITreeOperation
    {
        Tree Get(string oid, string basePath = "");

        void Read(string treeOid, bool updateWorkingDirectory = false);

        string Write();

        void CheckoutIndex(Tree index);

        Tree GetWorkingDirectory();

        IEnumerable<(string, string, string)> Iterate(string oid);

        Tree IndexTree { get; }
    }
}
