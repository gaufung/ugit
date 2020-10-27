namespace Ugit
{
    using System.Collections.Generic;

    /// <summary>
    /// Data provider for file operation.
    /// </summary>
    internal interface IDataProvider
    {
        string GitDirFullPath { get; }

        string GitDir { get;  }

        void Init();

        string HashObject(byte[] data, string type="blob");

        byte[] GetObject(string oid, string expected="blob");

        void UpdateRef(string @ref, RefValue value, bool deref=true);

        RefValue GetRef(string @ref, bool deref=true);

        IEnumerable<(string, RefValue)> IterRefs(string prefix = "", bool deref=true);

        void DeleteRef(string @ref, bool deref = true);

        Dictionary<string, string> GetIndex();

        void SetIndex(Dictionary<string, string> index);
    }
}
