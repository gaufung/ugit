using System.Collections.Generic;
using System.IO.Abstractions;
using System.Security.Cryptography;
using System.Text;

namespace ugit
{
    public static class Extensions
    {
        public static byte[] Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string Decode(this byte[] arr)
        {
            return Encoding.UTF8.GetString(arr);
        }

        public static string Sha1HexDigest(this byte[] data)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(data);
            var sb = new StringBuilder(hash.Length * 2);
            foreach (byte b in hash)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

        public static void CreateParentDirectory(this string filePath, IFileSystem fileSystem)
        {
            string folder = fileSystem.Path.GetDirectoryName(filePath);
            if (!fileSystem.Directory.Exists(folder))
            {
                fileSystem.Directory.CreateDirectory(folder);
            }
        }
        
        public static void Update<TKey, TValue>(this IDictionary<TKey, TValue> left, IDictionary<TKey, TValue> right)
        {
            foreach (var entry in right)
            {
                left[entry.Key] = entry.Value;
            }
        }
    }
}