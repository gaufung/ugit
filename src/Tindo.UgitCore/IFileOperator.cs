
using System.Collections.Generic;

namespace Tindo.UgitCore
{
    public interface IFileOperator
    {
        string CurrentDirectory { get;  }

        bool Exists(string path, bool isFile = true);

        bool TryRead(string path, out byte[] bytes);

        void Write(string filePath, byte[] data);

        void Delete(string path, bool isFile = true);

        void CreateDirectory(string directory, bool force = true);

        IEnumerable<string> Walk(string directory);
    }
}
