namespace Ugit
{
    /// <summary>
    /// Ref Value struct.
    /// </summary>
    internal struct RefValue
    {
        /// <summary>
        /// Gets or sets a value indicating whether it's symbolic.
        /// </summary>
        public bool Symbolic { get; set; }

        /// <summary>
        /// Gets or sets the ref value.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// Create a ref value struct.
        /// </summary>
        /// <param name="symbolic">Whether it is symbolic.</param>
        /// <param name="value">the symbol value.</param>
        /// <returns>The symbol struct.</returns>
        public static RefValue Create(bool symbolic, string value) => new RefValue { Symbolic = symbolic, Value = value };
    }
}
