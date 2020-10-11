﻿using CommandLine;

namespace Ugit.Options
{
    [Verb("hash-object", HelpText ="Hash an file.")]
    internal class HashObjectOption
    {
        [Value(0, Required=true)]
        public string File { get; set; }
    }
}