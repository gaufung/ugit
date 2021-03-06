﻿namespace Tindo.Ugit
{
    using System.Collections.Generic;

    /// <summary>
    /// Tag operation interface.
    /// </summary>
    internal interface ITagOperation
    {
        /// <summary>
        /// Gets all tags.
        /// </summary>
        IEnumerable<string> All { get; }

        /// <summary>
        /// Create an tag.
        /// </summary>
        /// <param name="name">tag name.</param>
        /// <param name="oid">tag oid.</param>
        void Create(string name, string oid);
    }
}
