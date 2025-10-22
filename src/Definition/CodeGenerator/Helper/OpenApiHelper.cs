using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace CodeGenerator.Helper;

public class OpenApiHelper
{
    /// <summary>
    /// 提取类型名及所有泛型参数类型名（原始 C# 类型全名），如 PageList`1[[A.B.C, ...]] -> PageList`1, A.B.C
    /// </summary>
    public static IEnumerable<string> ExtractAllTypeNames(string type)
    {
        if (string.IsNullOrWhiteSpace(type)) yield break;
        int genericTick = type.IndexOf('`');
        if (genericTick > 0)
        {
            yield return type.Split('[')[0];
            int start = type.IndexOf("[[");
            int end = type.LastIndexOf("]]");
            if (start > 0 && end > start)
            {
                var inner = type.Substring(start + 2, end - start - 2);
                var args = inner.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var arg in args)
                {
                    var argType = arg.Split(',')[0].Trim();
                    foreach (var sub in ExtractAllTypeNames(argType))
                        yield return sub;
                }
            }
            yield break;
        }
        yield return type.Trim();
    }
    public static TypeMeta ParseSchemaToTypeMeta(string schemaKey, IOpenApiSchema schema)
    {
        var meta = new TypeMeta
        {
            Name = FormatSchemaKey(schemaKey),
            Namespace = GetNamespace(schemaKey),
            FullName = schemaKey,
            Comment = schema.Description ?? schema.AllOf?.LastOrDefault()?.Description,
        };

        var enumExt = schema.Extensions?.FirstOrDefault(e => e.Key == "x-enumData").Value;
        if ((schema.Enum?.Count ?? 0) > 0 || enumExt is not null)
        {
            meta.IsEnum = true;
            meta.PropertyInfos = GetEnumProperties(schema);
            return meta;
        }

        var rootRef = GetRootRef(schema);
        if (!string.IsNullOrWhiteSpace(rootRef))
        {
            meta.IsReference = true;
            meta.ReferenceName = rootRef;
        }

        if (schema.Type == JsonSchemaType.Array)
        {
            meta.IsList = true;
        }

        if (schema.AdditionalProperties is not null)
        {
            var valType = MapToCSharpType(schema.AdditionalProperties);
            meta.FullName = $"Dictionary<string,{FormatSchemaKey(valType)}>";
            if (schema.AdditionalProperties is OpenApiSchemaReference valRef && valRef.Reference.Id is not null)
            {
                meta.IsReference = true;
                meta.ReferenceName = valRef.Reference.Id;
            }
        }

        if (schema.Properties?.Count > 0)
        {
            meta.PropertyInfos = ParseProperties(schema);
            if (schema.Required?.Count > 0)
            {
                foreach (var r in schema.Required)
                {
                    var p = meta.PropertyInfos.FirstOrDefault(p => p.Name == r);
                    p?.IsRequired = true;
                }
            }
        }

        if (schema.Type.HasValue && schema.Type.Value.HasFlag(JsonSchemaType.Null))
        {
            meta.IsNullable = true;
        }

        if (schemaKey.Contains('`'))
        {
            int start = schemaKey.IndexOf("[[");
            int end = schemaKey.LastIndexOf("]]");
            if (start > 0 && end > start)
            {
                var inner = schemaKey.Substring(start + 2, end - start - 2);
                var args = inner.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var arg in args)
                {
                    var argType = arg.Split(',')[0].Trim();
                    meta.GenericParams.Add(new TypeMeta
                    {
                        Name = FormatSchemaKey(argType),
                        Namespace = GetNamespace(argType),
                        FullName = argType
                    });
                }
                meta.FullName = ParseGenericFullName(schemaKey);
            }
        }
        return meta;
    }

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
                    if (item is not JsonObject obj) continue;
                    var name = obj["name"]?.GetValue<string>() ?? string.Empty;
                    var value = obj["value"]?.GetValue<int>() ?? 0;
                    var desc = obj["description"]?.GetValue<string>();
                    result.Add(new PropertyInfo
                    {
                        Name = name,
                        Type = "Enum(int)",
                        IsNullable = false,
                        CommentSummary = desc,
                        DefaultValue = value.ToString(),
                        IsEnum = true,
                        IsList = false,
                    });
                }
            }
        }
        return result;
    }

    public static List<PropertyInfo> ParseProperties(IOpenApiSchema schema)
    {
        var properties = new List<PropertyInfo>();
        if (schema.AllOf?.Count > 1)
        {
            properties.AddRange(ParseProperties(schema.AllOf[1]));
        }
        if (schema.Properties?.Count > 0)
        {
            var requiredList = schema.Required ?? new HashSet<string>();
            foreach (var prop in schema.Properties)
            {
                string type = MapToCSharpType(prop.Value);
                string name = prop.Key;
                string? desc = prop.Value.Description;
                var schemaType = prop.Value.Type;
                bool isEnum = prop.Value.Enum?.Count > 0 || prop.Value.Extensions?.ContainsKey("x-enumData") == true;
                bool isList = schemaType == JsonSchemaType.Array;

                var isNavigation = false;
                var navigationName = string.Empty;

                bool isRequired = requiredList.Contains(name);
                bool isNullable = false;
                if (schemaType.HasValue && schemaType.Value.HasFlag(JsonSchemaType.Null))
                {
                    isRequired = false;
                    isNullable = true;
                }
                if (prop.Value is OpenApiSchemaReference reference)
                {
                    isNavigation = true;
                    navigationName = reference.Reference.Id ?? type;
                }
                if (prop.Value.Items is not null and OpenApiSchemaReference itemsRef)
                {
                    isNavigation = true;
                    navigationName = itemsRef.Reference.Id ?? MapToCSharpType(prop.Value.Items);
                }

                properties.Add(new PropertyInfo
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
                });
            }
        }
        return properties;
    }


    /// <summary>
    /// fullname to model Name
    /// remove generic and namespace
    /// </summary>
    /// <param name="schemaKey"></param>
    /// <returns></returns>
    public static string FormatSchemaKey(string? schemaKey)
    {
        if (schemaKey == null) return string.Empty;
        string type = schemaKey;
        int backtickIndex = type.IndexOf('`');
        if (backtickIndex > 0) type = type[..backtickIndex];
        int lastDotIndex = type.LastIndexOf('.');
        if (lastDotIndex >= 0) type = type[(lastDotIndex + 1)..];
        return type;
    }

    public static string GetNamespace(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return string.Empty;
        int genericTick = fullName.IndexOf('`');
        if (genericTick > 0)
        {
            fullName = fullName[..genericTick];
        }
        int lastDotIndex = fullName.LastIndexOf('.');
        if (lastDotIndex > 0)
        {
            return fullName[..lastDotIndex];
        }
        return string.Empty;
    }
    
    public static string GetNamespaceFirstPart(string ns)
    {
        var res = ns.Split('.', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return res ?? ns;
    }

    public static string ParseGenericFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName)) return fullName;
        var mainTypeMatch = Regex.Match(fullName, @"([^\.]+)`(\d+)");
        if (!mainTypeMatch.Success) return fullName;
        string typeName = mainTypeMatch.Groups[1].Value;
        int genericCount = int.Parse(mainTypeMatch.Groups[2].Value);
        string[] genericParams = genericCount == 1
            ? new[] { "T" }
            : Enumerable.Range(1, genericCount).Select(i => $"T{i}").ToArray();
        return $"{typeName}<{string.Join(",", genericParams)}>";
    }

    public static string MapToCSharpType(IOpenApiSchema? schema)
    {
        if (schema == null) return "object";
        if (schema is OpenApiSchemaReference reference)
        {
            return reference.Reference.Id ?? "object";
        }
        var removeNullType = schema.Type.GetValueOrDefault();
        if (removeNullType.HasFlag(JsonSchemaType.Null))
        {
            removeNullType &= ~JsonSchemaType.Null;
        }
        switch (removeNullType)
        {
            case JsonSchemaType.Boolean:
                return "bool";
            case JsonSchemaType.Integer:
                return "int";
            case JsonSchemaType.Number:
                return "double";
            case JsonSchemaType.String:
                return schema.Format switch
                {
                    "binary" => "IFile",
                    "date-time" => "DateTimeOffset",
                    _ => "string",
                };
            case JsonSchemaType.Array:
                var itemType = MapToCSharpType(schema.Items);
                return $"List<{FormatSchemaKey(itemType)}>";
            case JsonSchemaType.Object:
                if (schema.AdditionalProperties != null)
                {
                    var valType = MapToCSharpType(schema.AdditionalProperties);
                    return $"Dictionary<string,{FormatSchemaKey(valType)}>";
                }
                return "object";
            default:
                return "object";
        }
    }

    /// <summary>
    /// 提取 schema 的根引用类型(用于请求/响应模型快速关联).
    /// 优先次序: 自身引用 -> 数组元素引用 -> oneOf 首引用
    /// </summary>
    public static string? GetRootRef(IOpenApiSchema? schema)
    {
        if (schema == null) return null;
        if (schema is OpenApiSchemaReference r && r.Reference.Id is not null) return r.Reference.Id;
        if (schema.Items is OpenApiSchemaReference ar && ar.Reference.Id is not null) return ar.Reference.Id;
        if (schema.OneOf?.FirstOrDefault() is OpenApiSchemaReference one && one.Reference.Id is not null) return one.Reference.Id;
        return null;
    }
}
