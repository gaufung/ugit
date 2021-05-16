namespace Tindo.UgitCore
{
    public struct Author
    {
        public string Name { get; set; }

        public string Email { get; set; }

        public override string ToString()
        {
            return $"{Name} <{Email}>";
        }

        public static readonly Author DefaultAuthor = new Author() {Email = string.Empty, Name = "unknown"};
        
        public static void Parse(string str, ref Author author)
        {
            string[] segments = str.Split('<');
            if (segments.Length == 2)
            {
                author.Name = segments[0].Trim();
                author.Email = segments[1].Trim('>');
            }
            else
            {
                author = Author.DefaultAuthor;
            }
        }
    }
}
