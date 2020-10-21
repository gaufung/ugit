using System.Collections.Generic;
using System.Reflection.PortableExecutable;

namespace Ugit
{
    internal interface IDataProvider
    {
        string GitDirFullPath { get; }

        string GitDir { get;  }

        void Init();

        string HashObject(byte[] data, string type="blob");

        byte[] GetObject(string oid, string expected="blob");

        void UpdateRef(string @ref, RefValue value);

        RefValue GetRef(string @ref);

        IEnumerable<(string, RefValue)> IterRefs();
    }
}
