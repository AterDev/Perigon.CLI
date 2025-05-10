using System.Text.Json.Serialization;

namespace Share.Converters;
public class DoubleStringJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType == JsonTokenType.Number ? reader.GetDouble().ToString() : reader.GetString() ?? "";
    }


    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
            writer.WriteStringValue(value);
}