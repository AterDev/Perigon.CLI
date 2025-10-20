namespace CodeGenerator.Models;

/// <summary>
/// 语言类型名称格式化接口: 输入标准(C#)类型字符串与属性标记, 输出目标语言类型表达。
/// 标准类型指 OpenApiHelper / 解析阶段确定下来的 C# 基础 / 泛型 / 集合 / 字典 结构。
/// </summary>
public interface ITypeNameFormatter
{
    /// <summary>
    /// 将标准化(C#)类型字符串转换成目标语言的类型表示。
    /// </summary>
    /// <param name="csharpType">标准类型(如: string,int,List&lt;User&gt;,Dictionary&lt;string,int&gt;)</param>
    /// <param name="isEnum">是否枚举</param>
    /// <param name="isList">是否列表</param>
    /// <param name="isNullable">是否可空</param>
    /// <returns>目标语言类型字符串</returns>
    string Format(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false);
}

/// <summary>
/// TypeScript 类型名称格式化实现。
/// </summary>
public class TypeScriptTypeNameFormatter : ITypeNameFormatter
{
    private static readonly Dictionary<string, string> PrimitiveMap = new()
    {
        ["string"] = "string",
        ["char"] = "string",
        ["int"] = "number",
        ["long"] = "number",
        ["short"] = "number",
        ["double"] = "number",
        ["float"] = "number",
        ["decimal"] = "number",
        ["bool"] = "boolean",
        ["Guid"] = "string",
        ["DateTime"] = "string",
        ["DateTimeOffset"] = "string",
        ["DateOnly"] = "string",
        ["TimeOnly"] = "string",
        ["TimeSpan"] = "string",
        ["object"] = "any",
        ["IFile"] = "FormData",
    };

    public string Format(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false)
    {
        if (string.IsNullOrWhiteSpace(csharpType)) return "any";

        var tsType = Normalize(csharpType);

        if (isEnum)
        {
            tsType = StripGenericArity(tsType);
        }

        if (isList || IsListType(csharpType))
        {
            var element = ExtractGenericArgument(csharpType) ?? "any";
            element = Normalize(element);
            tsType = element + "[]";
        }

        if (IsDictionaryType(csharpType))
        {
            var valueType = ExtractDictionaryValueType(csharpType) ?? "any";
            valueType = Normalize(valueType);
            tsType = $"Record<string, {valueType}>";
        }

        if (isNullable && !tsType.Contains("| null"))
        {
            tsType += " | null";
        }
        return tsType;
    }

    private static string Normalize(string csharpType)
    {
        if (string.IsNullOrWhiteSpace(csharpType)) return "any";
        var cleaned = StripGenericArity(csharpType);
        if (PrimitiveMap.TryGetValue(cleaned, out var mapped)) return mapped;

        if (cleaned.StartsWith("List<"))
        {
            var elem = ExtractGenericArgument(cleaned) ?? "any";
            return Normalize(elem) + "[]";
        }
        if (IsDictionaryType(cleaned))
        {
            var val = ExtractDictionaryValueType(cleaned) ?? "any";
            return $"Record<string, {Normalize(val)}>{string.Empty}";
        }
        if (cleaned.EndsWith("[]"))
        {
            return Normalize(cleaned[..^2]) + "[]";
        }
        return cleaned;
    }

    private static bool IsListType(string type) => type.StartsWith("List<") || type.EndsWith("[]");
    private static bool IsDictionaryType(string type) => type.StartsWith("Dictionary<");

    private static string? ExtractGenericArgument(string type)
    {
        var lt = type.IndexOf('<');
        var gt = type.LastIndexOf('>');
        if (lt > 0 && gt > lt)
        {
            var inner = type.Substring(lt + 1, gt - lt - 1);
            // 仅取第一个泛型参数(列表)
            return inner.Split(',')[0].Trim();
        }
        if (type.EndsWith("[]")) return type[..^2];
        return null;
    }

    private static string? ExtractDictionaryValueType(string type)
    {
        if (!IsDictionaryType(type)) return null;
        var inner = type["Dictionary<".Length..];
        inner = inner.TrimEnd('>');
        var parts = inner.Split(',');
        if (parts.Length == 2) return parts[1].Trim();
        return null;
    }

    private static string StripGenericArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick > 0 ? name[..tick] : name;
    }
}
