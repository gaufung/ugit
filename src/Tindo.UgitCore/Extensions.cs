using System.Runtime.CompilerServices;

namespace Tindo.UgitCore
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;

    internal static class Extensions
    {
        /// <summary>
        /// Digest a byte array with sha1 algorithm.
        /// </summary>
        /// <param name="data">The data array.</param>
        /// <returns>The digest value.</returns>
        public static string Sha1HexDigest(this byte[] data)
        {
            using SHA1Managed sha1 = new();
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

        public static Uri AddQuery(this Uri uri, string name, string value)
        {
            var httpValueCollection = HttpUtility.ParseQueryString(uri.Query);

            httpValueCollection.Remove(name);
            httpValueCollection.Add(name, value);

            var ub = new UriBuilder(uri);
            ub.Query = httpValueCollection.ToString();

            return ub.Uri;
        }

        public static bool IsNullOrWhiteSpace(this string str) => string.IsNullOrWhiteSpace(str);
    }
}
