using System.Collections.Generic;

namespace ugit
{
    public struct Commit
    {
        public string Tree { get; set; }

        public List<string> Parents { get; set; } 

        public string Message { get; set; }
    }
}