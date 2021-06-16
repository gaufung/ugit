namespace Tindo.UgitCore
{
    using System;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Ref Value struct.
    /// </summary>
    [JsonConverter(typeof(RefValueJsonConverter))]
    public struct RefValue
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
        public static RefValue Create(bool symbolic, string value)
            => new() { Symbolic = symbolic, Value = value };
    }

    internal class RefValueJsonConverter : JsonConverter<RefValue>
    {
        public override RefValue Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            bool symbolic = false;
            string value = string.Empty;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return RefValue.Create(symbolic, value);
                }

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    if (string.Equals(propertyName, "symbolic", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        var tokenType = reader.TokenType;
                        if (tokenType == JsonTokenType.False)
                        {
                            symbolic = false;
                        }
                        if (tokenType == JsonTokenType.True)
                        {
                            symbolic = true;
                        }
                    }

                    else if (string.Equals(propertyName, "value", StringComparison.OrdinalIgnoreCase))
                    {
                        reader.Read();
                        if (reader.TokenType == JsonTokenType.String)
                        {
                            value = reader.GetString();
                        }
                    }
                    else
                    {
                        throw new ArgumentException("");
                    }
                }
            }

            throw new ArgumentException();
        }

        public override void Write(Utf8JsonWriter writer, RefValue value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("symbolic");
            writer.WriteBooleanValue(value.Symbolic);
            writer.WritePropertyName("value");
            writer.WriteStringValue(value.Value);
            writer.WriteEndObject();
        }
    }
}
