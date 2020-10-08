namespace Ugit
{
    internal interface IDataProvider
    {
        string GitDirFullPath { get; }

        string GitDir { get;  }

        void Init();

        string HashObject(byte[] data, string type="blob");

        byte[] GetObject(string oid, string expected="blob");
    }
}
