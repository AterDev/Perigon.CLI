using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ater.Common.Converters;

/// <summary>
/// Number To String
/// </summary>
public class NumberToStringJsonConverter : JsonConverter<string>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString();
        }
        else if (reader.TokenType == JsonTokenType.Number && typeToConvert.FullName == "System.String")
        {
            if (reader.TryGetInt64(out var longValue))
            {
                return longValue.ToString();
            }
            else
            {
                return reader.GetDouble().ToString();
            }
        }
        return default;
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}
