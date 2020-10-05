using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

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
        
        public static bool IsOnlyHex(this string str)
        {
            return Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        public static IEnumerable<string> Walk(this IFileSystem fileSystem, string directory)
        {
            List<string> filePaths =new List<string>();
            foreach (var filePath in fileSystem.Directory.EnumerateFiles(directory))
            {
                filePaths.Add(filePath);
            }
            
            foreach (var directoryPath in fileSystem.Directory.EnumerateDirectories(directory))
            {
                var subFilePaths = fileSystem.Walk(directoryPath);
                filePaths.AddRange(subFilePaths);
            }

            return filePaths;
        }
        
        public static T Pop<T>(this HashSet<T> set)
        {
            var item = set.First();
            set.Remove(item);
            return item;
        }
    }
}