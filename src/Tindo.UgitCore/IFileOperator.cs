
using System.Collections.Generic;
using System;

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

        void EmptyDirectory(Func<string, bool> ignore);
    }
}
