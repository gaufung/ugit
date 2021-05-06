﻿namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Tag option.
    /// </summary>
    [Verb("tag")]
    [ExcludeFromCodeCoverage]
    internal class TagOption
    {
        /// <summary>
        /// Gets or sets name.
        /// </summary>
        [Value(0, Required = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets oid.
        /// </summary>
        [Value(1, Required = false, Default = "@")]
        public string Oid { get; set; }
    }
}