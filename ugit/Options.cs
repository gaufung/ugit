using System.Diagnostics;
using CommandLine;

namespace ugit
{
    [Verb("init")]
    public class InitOptions
    {
        
    }

    [Verb("hash-object", HelpText = "")]
    public class HashObjectOptions
    {
        [Value(0)]
        public string File { get; set; }
    }

    [Verb("cat-file")]
    public class CatFileOptions
    {
        [Value(0)]
        public string @Object { get; set; }
    }

    [Verb("write-tree")]
    public class WriteTreeOptions
    {
        
    }

    [Verb("read-tree")]
    public class ReadTreeOptions
    {
        [Value(0)]
        public string Tree { get; set; }
    }

    [Verb("commit")]
    public class CommitOptions
    {
        [Option('m', "message", Required = true)]
        public string Message { get; set; }
    }

    [Verb("log")]
    public class LogOptions
    {
        [Value(0, Required = false, Default = "@")]
        public string Oid { get; set; }
    }

    [Verb("checkout")]
    public class CheckOutOptions
    {
        [Value(0, Required = true)]
        public string Commit { get; set; }
    }

    [Verb("tag")]
    public class TagOptions
    {
        [Value(0, Required = true)]
        public string Name { get; set; }
        
        [Value(1, Required = false, Default = "@")]
        public string Oid { get; set; }
    }

    [Verb("k")]
    public class KOptions
    {
        
    }

    [Verb("branch")]
    public class BranchOptions
    {
        [Value(0, Required = false)]
        public string Name { get; set; }

        [Value(1, Required = false, Default = "@")]
        public string StartPoint { get; set; }
    }
    
    [Verb("status")]
    public class StatusOptions
    {
        
    }

    [Verb("reset")]
    public class ResetOptions
    {
        [Value(0, Required = true)]
        public string Commit { get; set; }
    }

    [Verb("show")]
    public class ShowOptions
    {
        [Value(0, Required = false, Default = "@")]
        public string Commit { get; set; }
    }

    [Verb("diff")]
    public class DiffOptions
    {
        [Value(0, Required = false, Default = "@")]
        public string Commit { get; set; }
    }

    [Verb("merge")]
    public class MergeOptions
    {
        [Value(0)]
        public string Commit { get; set; }
    }

    [Verb("merge-base")]
    public class MergeBaseOptions
    {
        [Value(0)]
        public string Commit1 { get; set; }
        [Value(1)]
        public string Commit2 { get; set; }
    }
}