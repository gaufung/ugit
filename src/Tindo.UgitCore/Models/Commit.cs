namespace Tindo.UgitCore
{
    using System.Collections.Generic;

    /// <summary>
    /// The ugit commit struct.
    /// </summary>
    public struct Commit
    {
        /// <summary>
        /// Gets or sets the tree oid for this commit.
        /// </summary>
        public string Tree { get; set; }

        /// <summary>
        /// Gets or sets the parents for this commit.
        /// </summary>
        public IList<string> Parents { get; set; }

        /// <summary>
        /// Gets or sets the commit message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the Author.
        /// </summary>
        public Author Author { get; set; }
    }
}
