using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Unicode;

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
    }
}
