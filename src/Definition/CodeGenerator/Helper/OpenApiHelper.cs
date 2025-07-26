using System.Text.Json.Nodes;

namespace CodeGenerator.Helper;

public class OpenApiHelper
{
    /// <summary>
    /// 获取转换成ts的类型
    /// </summary>
    public static string ConvertToTypescriptType(IOpenApiSchema prop)
    {
        string? type = "any";
        switch (prop.Type)
        {
            case JsonSchemaType.Boolean:
                type = "boolean";
                break;
            case JsonSchemaType.Integer:
                type = prop.Enum?.Count > 0 ? prop.DynamicRef : "number";
                break;
            case JsonSchemaType.Number:
                type = "number";
                break;
            case JsonSchemaType.String:
                switch (prop.Format)
                {
                    case "guid":
                        break;
                    case "binary":
                        type = "formData";
                        break;
                    case "date-time":
                        type = "Date";
                        break;
                    default:
                        type = "string";
                        break;
                }
                break;
            case JsonSchemaType.Array:
                type =
                    prop.Items?.DynamicRef != null
                        ? prop.Items.DynamicRef + "[]"
                        : ConvertToTypescriptType(prop.Items) + "[]";
                break;
            default:
                type = prop.DynamicRef ?? "any";
                break;
        }
        if (prop.OneOf?.Count > 0)
        {
            type = prop.OneOf.First()?.DynamicRef;
        }
        if (prop.Required == null || prop.DynamicRef != null)
        {
            type += " | null";
        }
        return type ?? "any";
    }

    /// <summary>
    /// 统一获取参数类型和引用类型（TypeScript风格）
    /// </summary>
    public static (string type, string? refType) GetParamType(IOpenApiSchema? schema)
    {
        if (schema == null)
        {
            return (string.Empty, string.Empty);
        }
        string type = "any";
        string? refType = schema.DynamicRef;
        if (schema.DynamicRef != null)
        {
            return (schema.DynamicRef, schema.DynamicRef);
        }
        switch (schema.Type)
        {
            case JsonSchemaType.Boolean:
                type = "boolean";
                break;
            case JsonSchemaType.Integer:
                if (schema.Enum?.Count > 0)
                {
                    if (schema.DynamicRef != null)
                    {
                        type = schema.DynamicRef;
                        refType = schema.DynamicRef;
                    }
                }
                else
                {
                    type = "number";
                    refType = "number";
                }
                break;
            case JsonSchemaType.Number:
                type = "number";
                break;
            case JsonSchemaType.String:
                type = "string";
                if (!string.IsNullOrWhiteSpace(schema.Format))
                {
                    type = schema.Format switch
                    {
                        "binary" => "FormData",
                        "date-time" => "string",
                        _ => "string",
                    };
                }
                break;
            case JsonSchemaType.Array:
                if (schema.Items?.DynamicRef != null)
                {
                    refType = schema.Items.DynamicRef;
                    type = refType + "[]";
                }
                else if (schema.Items.Type != null)
                {
                    var itemType = schema.Items.Type;
                    refType = itemType switch
                    {
                        JsonSchemaType.Integer => "number",
                        _ => itemType.Value.ToString(),
                    };
                    type = refType + "[]";
                }
                else if (schema.Items.OneOf?.FirstOrDefault()?.DynamicRef != null)
                {
                    refType = schema.Items.OneOf?.FirstOrDefault()!.DynamicRef;
                    type = refType + "[]";
                }
                break;
            case JsonSchemaType.Object:
                if (schema.Properties?.Count > 0)
                {
                    var obj = schema.Properties.FirstOrDefault().Value;
                    if (obj != null && obj.Format == "binary")
                    {
                        type = "FormData";
                    }
                }
                if (schema.AdditionalProperties != null)
                {
                    (string inType, string? inRefType) = GetParamType(schema.AdditionalProperties);
                    refType = inRefType;
                    type = $"Map<string, {inType}>";
                }
                break;
            case JsonSchemaType.Null:
                type = "FormData";
                break;
            default:
                break;
        }
        if (schema.OneOf?.Count > 0)
        {
            type = schema.OneOf.First()?.DynamicRef ?? type;
            refType = schema.OneOf.First()?.DynamicRef;
        }
        return (type, refType);
    }

    /// <summary>
    /// 获取C#类型（用于CsharpModelGenerate）
    /// </summary>
    public static (string type, string? refType) GetCsharpParamType(IOpenApiSchema? schema)
    {
        if (schema == null)
        {
            return ("object", null);
        }
        string type = "object";
        string? refType = schema.DynamicRef;
        if (schema.DynamicRef != null)
        {
            return (schema.DynamicRef, schema.DynamicRef);
        }
        switch (schema.Type)
        {
            case JsonSchemaType.Boolean:
                type = "bool";
                break;
            case JsonSchemaType.Integer:
                type = "int";
                break;
            case JsonSchemaType.Number:
                type = "double";
                break;
            case JsonSchemaType.String:
                type = "string";
                break;
            case JsonSchemaType.Array:
                if (schema.Items.DynamicRef != null)
                {
                    refType = schema.Items.DynamicRef;
                    type = $"List<{refType}>";
                }
                else if (schema.Items.Type != null)
                {
                    var itemType = schema.Items.Type;
                    refType = itemType switch
                    {
                        JsonSchemaType.Integer => "int",
                        JsonSchemaType.Number => "double",
                        JsonSchemaType.Boolean => "bool",
                        _ => "object",
                    };
                    type = $"List<{refType}>";
                }
                break;
            case JsonSchemaType.Object:
                type = "object";
                break;
            default:
                break;
        }
        if (schema.OneOf?.Count > 0)
        {
            type = schema.OneOf.First()?.DynamicRef ?? type;
            refType = schema.OneOf.First()?.DynamicRef;
        }
        return (type, refType);
    }

    /// <summary>
    /// 统一属性提取（TypeScript/C#）
    /// </summary>
    public static List<PropertyInfo> ParseProperties(IOpenApiSchema schema, bool forCsharp = false)
    {
        var properties = new List<PropertyInfo>();
        if (schema.AllOf?.Count > 1)
        {
            properties.AddRange(ParseProperties(schema.AllOf[1], forCsharp));
        }
        if (schema.Properties?.Count > 0)
        {
            foreach (var prop in schema.Properties)
            {
                string type = forCsharp
                    ? GetCsharpParamType(prop.Value).type
                    : ConvertToTypescriptType(prop.Value);
                string name = prop.Key;
                string? desc = prop.Value.Description;
                bool isNullable = prop.Value.Required == null;
                string? refType = forCsharp
                    ? GetCsharpParamType(prop.Value).refType
                    : prop.Value.DynamicRef;
                bool isEnum = prop.Value.Enum?.Count > 0;
                bool isList = prop.Value.Type == JsonSchemaType.Array;
                properties.Add(
                    new PropertyInfo
                    {
                        Name = name,
                        Type = type,
                        IsNullable = isNullable,
                        CommentSummary = desc,
                        IsEnum = isEnum,
                        IsList = isList,
                    }
                );
            }
        }
        return properties;
    }

    /// <summary>
    /// 统一枚举属性提取
    /// </summary>
    public static List<PropertyInfo> GetEnumProperties(IOpenApiSchema schema)
    {
        var result = new List<PropertyInfo>();
        var extEnumData = schema.Extensions?.FirstOrDefault(e => e.Key == "x-enumData");
        if (extEnumData.HasValue)
        {
            var data = extEnumData.Value.Value as JsonNodeExtension;
            if (data?.Node is JsonArray array)
            {
                foreach (var item in array)
                {
                    if (item is not JsonObject itemObj)
                    {
                        continue;
                    }
                    var name = item["name"]?.GetValue<string>() ?? string.Empty;
                    var value = item["value"]?.GetValue<int>() ?? 0;
                    var desc = item["description"]?.GetValue<string>();
                    result.Add(
                        new PropertyInfo
                        {
                            Name = name,
                            Type = "Enum:int",
                            IsNullable = false,
                            CommentSummary = desc,
                            DefaultValue = value.ToString(),
                            IsEnum = true,
                            IsList = false,
                        }
                    );
                }
            }
        }
        return result;
    }
}
