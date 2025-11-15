namespace Ater.AspNetCore.Converters;

public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options
    )
    {
        return reader.TryGetDateTime(out DateTime dateTime)
            ? DateOnly.FromDateTime(dateTime.ToLocalTime())
            : throw new JsonException("Invalid date format.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
    }
}
