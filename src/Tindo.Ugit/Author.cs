namespace Tindo.Ugit
{
    internal record Author(string Name, string Email)
    {
        /// <inheritdoc/>
        public override string ToString() => $"{this.Name} <{this.Email}>";

        public static bool TryParse(string value, out Author author)
        {
            value = value.Trim();
            string[] tokens = value.Split(' ');
            if (tokens.Length != 2)
            {
                author = null;
                return false;
            }

            string name = tokens[0];
            string email = tokens[1].TrimStart('<').TrimEnd('>');
            author = new Author(name, email);
            return true;
        }
    }
}
