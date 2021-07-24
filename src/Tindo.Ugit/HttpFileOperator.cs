using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tindo.Ugit
{
    /// <summary>
    /// Http File Operator.
    /// </summary>
    public class HttpFileOperator : IFileOperator
    {
        public string CurrentDirectory => throw new NotImplementedException();

        public void CreateDirectory(string directory, bool force = true)
        {
            throw new NotImplementedException();
        }

        public void Delete(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        public void EmptyCurrentDirectory(Func<string, bool> ignore)
        {
            throw new NotImplementedException();
        }

        public bool Exists(string path, bool isFile = true)
        {
            throw new NotImplementedException();
        }

        public byte[] Read(string path)
        {
            throw new NotImplementedException();
        }

        public bool TryRead(string path, out byte[] bytes)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<string> Walk(string path)
        {
            throw new NotImplementedException();
        }

        public void Write(string path, byte[] bytes)
        {
            throw new NotImplementedException();
        }
    }
}
