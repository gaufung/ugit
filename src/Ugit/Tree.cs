namespace Tindo.Ugit
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Tree represents the ugit tree object. 
    /// </summary>
    [JsonConverter(typeof(TreeJsonConverter))]
    public class Tree : IEnumerable<KeyValuePair<string, string>>
    {
        private readonly IDictionary<string, string> dict;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class.
        /// </summary>
        public Tree()
        {
            this.dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tree"/> class.
        /// </summary>
        /// <param name="dict">The default dictionary.</param>
        public Tree(IDictionary<string, string> dict)
        {
            this.dict = new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Get enumertor for foreach statement
        /// </summary>
        /// <returns>Ienueratble key-value pair.</returns>
        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var keyValue in this.dict)
            {
                yield return keyValue;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.dict.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets the indexer
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>the value.</returns>
        public string this[string key]
        {
            get => this.dict[key];
            set => this.dict[key] = value;
        }

        /// <summary>
        /// Clear
        /// </summary>
        public void Clear() => this.dict.Clear();

        /// <summary>
        /// Gets the count of Tree's entry.
        /// </summary>
        public int Count => this.dict.Count;

        /// <summary>
        /// Contains a key in the tree.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns>True if the tree contains the key.</returns>
        public bool ContainsKey(string key) => this.dict.ContainsKey(key);

        /// <summary>
        /// Add method. 
        /// </summary>
        /// <param name="key">The key</param>
        /// <param name="value">the value.</param>
        public void Add(string key, string value)
        {
            this.dict[key] = value;
        }

        /// <summary>
        /// Update with other tree.
        /// </summary>
        /// <param name="other">other tree.</param>
        public void Update(Tree other)
        {
            foreach (var entry in other)
            {
                this[entry.Key] = entry.Value;
            }
        }

    }

    /// <summary>
    /// Json Converter
    /// </summary>
    internal class TreeJsonConverter : JsonConverter<Tree>
    {
        public override Tree Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            Tree tree = new Tree();
            string key = string.Empty;
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonTokenType.StartObject:
                    case JsonTokenType.EndObject:
                        break;
                    case JsonTokenType.PropertyName:
                        key = reader.GetString();
                        break;
                    case JsonTokenType.String:
                        string value = reader.GetString();
                        tree[key] = value;
                        break;
                }
            }

            return tree;
        }

        public override void Write(Utf8JsonWriter writer, Tree value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            foreach (var entry in value)
            {
                writer.WriteString(entry.Key, entry.Value);
            }

            writer.WriteEndObject();
        }
    }
}