namespace ugit
{
    public struct RefValue
    {
        public bool Symbolic { get; set; }

        public string Value { get; set; }
        
        public static RefValue Create(bool symbolic, string value)
        =>new RefValue(){Symbolic = symbolic, Value = value};
    }
}