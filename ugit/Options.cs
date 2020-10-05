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
        [Value(0, Required = false)]
        public string Oid { get; set; }
    }
}