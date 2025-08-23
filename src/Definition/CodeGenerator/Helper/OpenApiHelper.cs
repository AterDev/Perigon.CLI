using System.Text.Json.Nodes;

namespace CodeGenerator.Helper;

public class OpenApiHelper
{
    /// <summary>
    /// parse params type
    /// </summary>
    /// <param name="schema"></param>
    /// <returns>
    /// <![CDATA[type is full type name,like: List<Type>,Map<string, Type>]]>,
    /// refType is the reference type name,like: Type
    /// </returns>
    public static (string type, string? refType) ParseParamTSType(IOpenApiSchema? schema)
    {
        if (schema == null)
        {
            return (string.Empty, string.Empty);
        }
        string type = "any";
        string? refType = "any";
        if (schema is OpenApiSchemaReference reference)
        {
            return (reference.Reference.Id ?? "any", reference.Reference.Id);
        }
        if (schema.Type == JsonSchemaType.Null)
        {
            return (type, refType);
        }
        var removeNullType = schema.Type.GetValueOrDefault();
        if (removeNullType.HasFlag(JsonSchemaType.Null))
        {
            // remove null type
            removeNullType &= ~JsonSchemaType.Null;
        }
        switch (removeNullType)
        {
            case JsonSchemaType.Boolean:
                refType = type = "boolean";
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
                    refType = type = "number";
                }
                break;
            case JsonSchemaType.Number:
                refType = type = "number";
                break;
            case JsonSchemaType.String:
                type = refType = "string";
                if (!string.IsNullOrWhiteSpace(schema.Format))
                {
                    refType = type = schema.Format switch
                    {
                        "binary" => "FormData",
                        "date-time" => "string",
                        _ => "string",
                    };
                }
                break;
            case JsonSchemaType.Array:
                if (schema.Items is OpenApiSchemaReference enumReference)
                {
                    refType = enumReference.Reference.Id ?? "any";
                    type = refType + "[]";
                }
                else if (schema.Items?.Type != null)
                {
                    var itemType = schema.Items.Type;
                    refType = itemType switch
                    {
                        JsonSchemaType.Integer => "number",
                        _ => itemType.Value.ToString(),
                    };
                    type = refType + "[]";
                }
                else if (schema.Items?.OneOf?.FirstOrDefault()?.DynamicRef != null)
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
                        type = refType = "FormData";
                    }
                }
                if (schema.AdditionalProperties != null)
                {
                    (string inType, string? inRefType) = ParseParamTSType(
                        schema.AdditionalProperties
                    );
                    refType = inRefType;
                    type = $"Map<string, {inType}>";
                }
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
    /// parse params type
    /// </summary>
    /// <param name="schema"></param>
    /// <returns>
    /// <![CDATA[type is full type name,like: List<Type>,Map<string, Type>]]>,
    /// refType is the reference type name,like: Type
    /// </returns>
    public static (string type, string? refType) ParseParamAsCSharp(IOpenApiSchema? schema)
    {
        if (schema == null)
        {
            return ("object", null);
        }
        string type = "object";
        string refType = "object";
        if (schema is OpenApiSchemaReference reference)
        {
            return (reference.Reference.Id ?? "object", reference.Reference.Id);
        }
        if (schema.Type == JsonSchemaType.Null)
        {
            return (type, refType);
        }
        var removeNullType = schema.Type.GetValueOrDefault();
        if (removeNullType.HasFlag(JsonSchemaType.Null))
        {
            // remove null type
            removeNullType &= ~JsonSchemaType.Null;
        }
        switch (removeNullType)
        {
            case JsonSchemaType.Boolean:
                type = refType = "bool";
                break;
            case JsonSchemaType.Integer:
                type = refType = "int";
                break;
            case JsonSchemaType.Number:
                type = refType = "double";
                break;
            case JsonSchemaType.String:
                type = refType = "string";
                if (!string.IsNullOrWhiteSpace(schema.Format))
                {
                    refType = type = schema.Format switch
                    {
                        "binary" => "IFile",
                        "date-time" => "DateTimeOffset",
                        _ => "string",
                    };
                }
                break;
            case JsonSchemaType.Array:
                if (schema.Items is OpenApiSchemaReference enumReference)
                {
                    refType = enumReference.Reference.Id ?? "object";
                    type = $"List<{refType}>";
                }
                else if (schema.Items?.Type != null)
                {
                    var itemType = schema.Items.Type;
                    refType = itemType switch
                    {
                        JsonSchemaType.Integer => "int",
                        JsonSchemaType.Number => "double",
                        JsonSchemaType.Boolean => "bool",
                        JsonSchemaType.String => "string",
                        _ => "object",
                    };
                    type = $"List<{refType}>";
                }
                break;
            case JsonSchemaType.Object:
                if (schema.Properties?.Count > 0)
                {
                    var obj = schema.Properties.FirstOrDefault().Value;
                    if (obj != null && obj.Format == "binary")
                    {
                        type = "IFile";
                    }
                }
                if (schema.AdditionalProperties != null)
                {
                    (string inType, string? inRefType) = ParseParamTSType(
                        schema.AdditionalProperties
                    );
                    refType = inRefType ?? refType;
                    type = $"Dictionary<string, {inType}>";
                }
                break;
            default:
                break;
        }
        if (
            schema.OneOf != null
            && schema.OneOf.FirstOrDefault() is OpenApiSchemaReference reference1
        )
        {
            type = reference1.Reference.Id ?? type;
            refType = reference1.Reference.Id ?? refType;
        }
        return (type, refType);
    }

    /// <summary>
    /// parse properties from OpenAPI schema
    /// default use C# types, set useTypescript to true for TypeScript types
    /// </summary>
    public static List<PropertyInfo> ParseProperties(
        IOpenApiSchema schema,
        bool useTypescript = false
    )
    {
        var properties = new List<PropertyInfo>();
        if (schema.AllOf?.Count > 1)
        {
            properties.AddRange(ParseProperties(schema.AllOf[1], useTypescript));
        }
        if (schema.Properties?.Count > 0)
        {
            foreach (var prop in schema.Properties)
            {
                var (type, refType) = useTypescript
                    ? ParseParamTSType(prop.Value)
                    : ParseParamAsCSharp(prop.Value);
                string name = prop.Key;
                string? desc = prop.Value.Description;
                var schemaType = prop.Value.Type;
                bool isNullable = prop.Value.Required == null;
                bool isEnum = prop.Value.Enum?.Count > 0;
                bool isList = schemaType == JsonSchemaType.Array;

                var isNavigation = false;
                var navigationName = string.Empty;
                var isRequired = true;

                if (schemaType.HasValue && schemaType.Value.HasFlag(JsonSchemaType.Null))
                {
                    isRequired = false;
                }

                if (prop.Value is OpenApiSchemaReference reference)
                {
                    isNavigation = true;
                    navigationName = reference.Reference.Id ?? type;
                }
                if (prop.Value.Items is not null and OpenApiSchemaReference reference1)
                {
                    isNavigation = true;
                    navigationName = reference1.Reference.Id ?? refType;
                }
                properties.Add(
                    new PropertyInfo
                    {
                        Name = name,
                        Type = type,
                        IsNullable = isNullable,
                        CommentSummary = desc,
                        IsEnum = isEnum,
                        IsList = isList,
                        IsNavigation = isNavigation,
                        NavigationName = navigationName,
                        IsRequired = isRequired,
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
