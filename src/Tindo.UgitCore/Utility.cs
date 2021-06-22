using System.IO;
using System.Linq;

namespace Tindo.UgitCore
{
    public class Utility
    {
        public static bool IsIgnore(string path)
            => path.Split(Path.DirectorySeparatorChar).Contains(Constants.GitDir);
    }
}