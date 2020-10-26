namespace Ugit
{
    using System.Collections.Generic;

    internal struct Commit
    {
        public string Tree { get; set; }

        public List<string> Parents { get; set; }

        public string Message { get; set; }
    }
}
