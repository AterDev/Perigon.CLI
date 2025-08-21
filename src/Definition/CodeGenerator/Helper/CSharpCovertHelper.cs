using System.Text.Json;

namespace CodeGenerator.Helper;

/// <summary>
/// csharp 转换帮助
/// </summary>
public class CSharpCovertHelper
{
    public List<string> ClassCodes { get; set; } = [];
    public List<JsonMetadata> JsonMetadataList { get; } = [];

    /// <summary>
    /// 判断是否为合法的 json
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static bool CheckJson(string json)
    {
        try
        {
            object? obj = JsonSerializer.Deserialize<object>(json);
            return obj != null;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// json转C#模型类
    /// </summary>
    /// <param name="jsonElement"></param>
    /// <param name="className"></param>
    /// <returns></returns>
    public void GenerateClass(JsonElement jsonElement, string className = "Model")
    {
        className = className.ToPascalCase();
        StringBuilder sb = new();
        sb.AppendLine($"public class {className}");
        sb.AppendLine("{");
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            jsonElement = jsonElement.EnumerateArray().First();
        }

        foreach (JsonProperty property in jsonElement.EnumerateObject())
        {
            string propertyName = property.Name;
            JsonElement propertyValue = property.Value;
            string defaultValue = "";

            string csharpType = ConvertToType(propertyValue, propertyName);
            if (propertyValue.ValueKind == JsonValueKind.Array)
            {
                defaultValue = "[]";
            }

            if (propertyName.ToUpperFirst() != propertyName.ToPascalCase())
            {
                sb.AppendLine($"    [JsonPropertyName(\"{propertyName}\")]");
            }

            // 处理非法类型或变量名称
            if (int.TryParse(csharpType, out _))
            {
                csharpType = "The" + csharpType;
            }
            if (int.TryParse(propertyName, out _))
            {
                propertyName = "The" + propertyName;
            }

            string propertyLine =
                $"public {csharpType} {propertyName.ToPascalCase()} {{ get; set; }}";
            if (!string.IsNullOrEmpty(defaultValue))
            {
                propertyLine += $" = {defaultValue};";
            }
            sb.AppendLine($"    {propertyLine}");

            // 对象处理
            if (propertyValue.ValueKind == JsonValueKind.Object)
            {
                GenerateClass(propertyValue, propertyName);
            }
            // 数组处理
            else if (
                propertyValue.ValueKind == JsonValueKind.Array
                && propertyValue.GetArrayLength() > 0
                && propertyValue[0].ValueKind == JsonValueKind.Object
            )
            {
                GenerateClass(propertyValue[0], propertyName);
            }
        }
        sb.AppendLine("}");
        ClassCodes.Add(sb.ToString());
    }

    public void ToCsharpClassContent(JsonElement jsonElement)
    {
        ConvertJsonToMetadata(jsonElement);
        // TODO:合并处理
        // TODO:生成类
    }

    public JsonMetadata ConvertJsonToMetadata(JsonElement jsonElement, string? rootName = "Model")
    {
        JsonMetadata metadata = new() { Name = rootName };

        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            metadata.Type = JsonMetadata.JsonMetadataType.Object;
            metadata.Descents = [];

            foreach (JsonProperty property in jsonElement.EnumerateObject())
            {
                JsonMetadata child = ConvertJsonToMetadata(
                    property.Value,
                    property.Name.ToPascalCase()
                );
                child.Parent = metadata;
                metadata.Descents.Add(child);
            }
            JsonMetadataList.Add(metadata);
        }
        else if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            metadata.Type = JsonMetadata.JsonMetadataType.Array;
            if (jsonElement.GetArrayLength() > 0)
            {
                JsonElement firstChild = jsonElement.EnumerateArray().First();
                JsonMetadata child = ConvertJsonToMetadata(firstChild, metadata.Name);
                child.Parent = metadata;
                metadata.Descents.Add(child);
            }
        }
        else
        {
            metadata.Type = JsonMetadata.JsonMetadataType.KeyValue;
            metadata.ValueType = ConvertToType(jsonElement);
        }

        return metadata;
    }

    private static string ConvertToType(JsonElement element, string? propertyName = null)
    {
        // 类型处理
        var csharpType = element.ValueKind switch
        {
            JsonValueKind.Number => "int",
            JsonValueKind.String => "string?",
            JsonValueKind.True => "bool",
            JsonValueKind.False => "bool",
            JsonValueKind.Object => propertyName?.ToPascalCase(),
            JsonValueKind.Array => $"List<{propertyName?.ToPascalCase()}>",
            JsonValueKind.Null => "object?",
            _ => "object",
        };
        if (element.ValueKind == JsonValueKind.Number)
        {
            if (element.TryGetDouble(out _))
            {
                csharpType = "double";
            }

            if (element.TryGetInt32(out _))
            {
                csharpType = "int";
            }

            if (element.TryGetInt64(out _))
            {
                csharpType = "long";
            }
        }
        if (element.ValueKind == JsonValueKind.String)
        {
            if (element.TryGetGuid(out _))
            {
                csharpType = "Guid";
            }
            if (element.TryGetDateTime(out _))
            {
                csharpType = "DateTime";
            }
            if (element.TryGetDateTimeOffset(out DateTimeOffset dateTimeOffset))
            {
                csharpType = "DateTimeOffset";
                // if this is a DateOnly
                if (dateTimeOffset.TimeOfDay == TimeSpan.Zero)
                {
                    csharpType = "DateOnly";
                }
            }
            string? stringValue = element.GetString();
            string[] formats = ["HH:mm:ss", "HH:mm"];
            if (
                DateTime.TryParseExact(
                    stringValue,
                    formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _
                )
            )
            {
                csharpType = "TimeOnly";
            }
            formats = ["yyyy-MM-dd HH:mm:ss", "yyyy/MM/dd HH:mm:ss"];
            if (
                DateTime.TryParseExact(
                    stringValue,
                    formats,
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out _
                )
            )
            {
                csharpType = "DateTime";
            }
        }
        return csharpType ?? "object";
    }
}

/// <summary>
/// json 解析的元信息
/// </summary>
public class JsonMetadata
{
    /// <summary>
    /// 键名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 子级元素
    /// </summary>
    public List<JsonMetadata> Descents { get; set; } = [];

    /// <summary>
    /// 父属性
    /// </summary>
    public JsonMetadata? Parent { get; set; }

    /// <summary>
    /// 值类型
    /// </summary>
    public string? ValueType { get; set; }

    /// <summary>
    /// 可空
    /// </summary>
    public bool Nullable { get; set; }

    public JsonMetadataType Type { get; set; }

    public enum JsonMetadataType
    {
        Object,
        Array,
        Dictionary,
        KeyValue,
    }
}
