using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Tindo.UgitCore.Operations;

namespace Tindo.UgitCore
{
    /// <summary>
    /// Tree represents the ugit tree object. 
    /// </summary>
    public class Tree : IEnumerable<KeyValuePair<string, string>>
    {
        private IDictionary<string, string> _dict;

        public Tree()
        {
            _dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public Tree(IDictionary<string, string> dict)
        {
            _dict = new Dictionary<string, string>(dict, StringComparer.OrdinalIgnoreCase);
        }


        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            foreach (var keyValue in this._dict)
            {
                yield return keyValue;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dict.GetEnumerator();
        }

        public string this[string key]
        {
            get => _dict[key];
            set => _dict[key] = value;
        }

        public void Clear() => this._dict.Clear();

        public int Count => this._dict.Count;

        public bool ContainsKey(string key) => this._dict.ContainsKey(key);

        public void Add(string key, string value)
        {
            _dict[key] = value;
        }
        
    }

    internal class TreeJsonConverter : JsonConverter<Tree>
    {
        public override Tree? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
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