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
}