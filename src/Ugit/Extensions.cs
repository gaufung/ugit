using System.Linq;
using System.Security.Cryptography;

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
    }
}
