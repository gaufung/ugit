using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Ugit
{
    internal static class Extensions
    {
        public static string Sha1HexDigest(this byte[] data)
        {
            using var sha1 = new SHA1Managed();
            var hash = sha1.ComputeHash(data);
            return string.Join("", hash.Select(h => h.ToString("x2")));
        }

        public static byte[] Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        public static string Decode(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        public static IDictionary<TKey, TValue> Update<TKey, TValue>(this IDictionary<TKey, TValue> dic,
            IDictionary<TKey, TValue> other)
        {
            foreach (var entry in other)
            {
                dic[entry.Key] = entry.Value;
            }
            return dic;
        }

        public static void CreateParentDirectory(this IFileSystem fileSystem, string filePath)
        {
            string parentDirectory = Path.GetDirectoryName(filePath);
            if(!string.IsNullOrWhiteSpace(parentDirectory) && !fileSystem.Directory.Exists(parentDirectory))
            {
                fileSystem.Directory.CreateDirectory(parentDirectory);
            }
        }

        public static bool IsOnlyHex(this string str)
        {
            if(string.IsNullOrEmpty(str))
                return false;
            return Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static IEnumerable<string> Walk(this IFileSystem fileSystem, string directory)
        {
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                yield return filePath;
            }

            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                foreach (var filepath in fileSystem.Walk(directoryPath))
                {
                    yield return filepath;
                }
            }
        }
    }
}
