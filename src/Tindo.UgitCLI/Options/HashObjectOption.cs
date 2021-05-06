﻿namespace Tindo.UgitCLI.Options
{
    using System.Diagnostics.CodeAnalysis;
    using CommandLine;

    /// <summary>
    /// Hash object option.
    /// </summary>
    [Verb("hash-object", HelpText ="Hash an file.")]
    [ExcludeFromCodeCoverage]
    internal class HashObjectOption
    {
        /// <summary>
        /// Gets or sets the file.
        /// </summary>
        [Value(0, Required=true)]
        public string File { get; set; }
    }
}