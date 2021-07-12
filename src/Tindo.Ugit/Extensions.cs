namespace Tindo.Ugit
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Abstractions;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The ugit extension methods.
    /// </summary>
    internal static class Extensions
    {
        /// <summary>
        /// Digest a byte array with sha1 algorithm.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <returns>The digest value.</returns>
        public static string Sha1HexDigest(this byte[] data)
        {
            using SHA1Managed sha1 = new ();
            var hash = sha1.ComputeHash(data);
            return string.Join(string.Empty, hash.Select(h => h.ToString("x2")));
        }

        /// <summary>
        /// Encode string to byte array with UTF8.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>The encode byte array.</returns>
        public static byte[] Encode(this string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }

        /// <summary>
        /// Decode byte array to string with UTF8.
        /// </summary>
        /// <param name="data">The byte array.</param>
        /// <returns>The decode string.</returns>
        public static string Decode(this byte[] data)
        {
            return Encoding.UTF8.GetString(data);
        }

        /// <summary>
        /// Update a dictionary with another dictionary.
        /// </summary>
        /// <typeparam name="TKey">The dictionary key type.</typeparam>
        /// <typeparam name="TValue">The dictionary value type.</typeparam>
        /// <param name="dic">The dictionary.</param>
        /// <param name="other">The other dictionary.</param>
        public static void Update<TKey, TValue>(
            this IDictionary<TKey, TValue> dic,
            IDictionary<TKey, TValue> other)
        {
            foreach (var entry in other)
            {
                dic[entry.Key] = entry.Value;
            }
        }

        /// <summary>
        /// Create parent directionary for given filepath.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="filePath">the file path.</param>
        public static void CreateParentDirectory(this IFileSystem fileSystem, string filePath)
        {
            string parentDirectory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory) && !fileSystem.Directory.Exists(parentDirectory))
            {
                fileSystem.Directory.CreateDirectory(parentDirectory);
            }
        }

        /// <summary>
        /// Whether a string contains hex character.
        /// </summary>
        /// <param name="str">the string.</param>
        /// <returns>True if all character is hex format.</returns>
        public static bool IsOnlyHex(this string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            return Regex.IsMatch(str, @"\A\b[0-9a-fA-F]+\b\Z");
        }

        /// <summary>
        /// Walk through a dicrionary recursively.
        /// </summary>
        /// <param name="fileSystem">The file system.</param>
        /// <param name="directory">The directory.</param>
        /// <returns>The all files path.</returns>
        public static IEnumerable<string> Walk(this IFileSystem fileSystem, string directory)
        {
            if (!fileSystem.Directory.Exists(directory))
            {
                yield break;
            }

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

        /// <summary>
        /// Get refs mapping.
        /// </summary>
        /// <param name="dataProvider">the data provider.</param>
        /// <param name="prefix">the ref prefix.</param>
        /// <returns>The ref mapping { ref, ref-value }</returns>
        public static IDictionary<string, string> GetRefsMapping(this IDataProvider dataProvider, string prefix)
        {
            Dictionary<string, string> refs = new ();
            foreach (var (refname, @ref) in dataProvider.GetAllRefs(prefix))
            {
                refs.Add(refname, @ref.Value);
            }

            return refs;
        }
    }
}
