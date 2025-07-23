using System.Collections.Frozen;
using System.Text.Json.Nodes;
using Microsoft.OpenApi;

namespace CodeGenerator.Helper;

public class OpenApiHelper
{
    /// <summary>
    /// 获取枚举扩展数据
    /// </summary>
    /// <param name="extension"></param>
    /// <returns></returns>
    public static FrozenDictionary<string, int>? ParseEnumExtension(
        KeyValuePair<string, IOpenApiExtension> extension
    )
    {
        if (extension.Value != null)
        {
            var data = extension.Value as JsonNode as JsonArray;
            if (data != null && data.Count > 0)
            {
                return data.ToFrozenDictionary(
                    x => x!["name"]?.GetValue<string>() ?? "",
                    x => x!["value"]?.GetValue<int>() ?? 0
                );
            }
        }
        return default;
    }

    /// <summary>
    /// 获取转换成ts的类型
    /// </summary>
    /// <param name="prop"></param>
    /// <returns></returns>
    public static string ConvertToTypescriptType(OpenApiSchema prop)
    {
        string? type = "any";
        // 常规类型
        switch (prop.Type)
        {
            case JsonSchemaType.Boolean:
                type = "boolean";
                break;

            case JsonSchemaType.Integer:
                // 看是否为enum
                type = prop.Enum.Count > 0 ? prop.Reference?.Id : "number";
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
                    prop.Items.Reference != null
                        ? prop.Items.Reference.Id + "[]"
                        : ConvertToTypescriptType(prop.Items) + "[]";
                break;
            default:
                type = prop.Reference?.Id ?? "any";
                break;
        }
        // 引用对象
        if (prop.OneOf.Count > 0)
        {
            // 获取引用对象名称
            type = prop.OneOf.First()?.Reference.Id;
        }

        if (prop.Nullable || prop.Reference != null)
        {
            type += " | null";
        }

        return type ?? "any";
    }
}
