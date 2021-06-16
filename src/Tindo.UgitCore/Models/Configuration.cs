using System.Collections.Generic;

namespace Tindo.UgitCore
{
    public class Configuration
    {
        public Author Author { get; set; }

        public IList<Remote> Remotes { get; set; }
    }
}
