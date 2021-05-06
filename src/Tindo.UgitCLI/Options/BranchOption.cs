﻿namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Branch Option.
    /// </summary>
    [Verb("branch")]
    [ExcludeFromCodeCoverage]
    internal class BranchOption
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        [Value(0, Required = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or set the Start point.
        /// </summary>
        [Value(1, Required = false, Default = "@")]
        public string StartPoint { get; set; }
    }
}