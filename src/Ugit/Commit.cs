namespace Ugit
{
    using System.Collections.Generic;

    /// <summary>
    /// The ugit commit struct.
    /// </summary>
    internal struct Commit
    {
        /// <summary>
        /// Gets or sets the tree oid for this commit.
        /// </summary>
        public string Tree { get; set; }

        /// <summary>
        /// Gets or sets the parents for this commit.
        /// </summary>
        public List<string> Parents { get; set; }

        /// <summary>
        /// Gets or sets the commit messge.
        /// </summary>
        public string Message { get; set; }
    }
}
