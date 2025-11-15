namespace Ater.AspNetCore.Converters;

/// <summary>
/// Number To String
/// </summary>
public class NumberToStringJsonConverter : JsonConverter<string>
{
    public override string? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        else if (
            reader.TokenType == JsonTokenType.Number
            && typeToConvert.FullName == "System.String"
        )
        {
            return reader.TryGetInt64(out var longValue)
                ? longValue.ToString()
                : reader.GetDouble().ToString();
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
