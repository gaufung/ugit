using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tindo.UgitCore
{
    public interface IDataOperator
    {
        byte[] GetObject(string oid, string expected = "blob");

        string WriteObject(byte[] data, string type = "blob");

        void Initialize();
    }
}
