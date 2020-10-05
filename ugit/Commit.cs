namespace ugit
{
    public struct Commit
    {
        public string Tree { get; set; }

        public string Parent { get; set; }

        public string Message { get; set; }
    }
}