namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// 抽象语言格式化器, 提供公共辅助方法。
/// </summary>
public abstract class LanguageFormatterBase : ILanguageFormatter
{
    public abstract string FormatType(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false);
    public abstract string GenerateModel(TypeMeta meta);

    /// <summary>
    /// 复用 OpenApiHelper.FormatSchemaKey, 便于派生类调用 (包装一层, 方便未来替换/缓存)。
    /// </summary>
    public string FormatSchemaKey(string name) => OpenApiHelper.FormatSchemaKey(name);

    #region shared helpers
    protected static string StripGenericArity(string name)
    {
        var tick = name.IndexOf('`');
        return tick > 0 ? name[..tick] : name;
    }
    protected static bool IsListType(string type) => type.StartsWith("List<") || type.EndsWith("[]");
    protected static bool IsDictionaryType(string type) => type.StartsWith("Dictionary<");
    protected static string? ExtractGenericArgument(string type)
    {
        var lt = type.IndexOf('<');
        var gt = type.LastIndexOf('>');
        if (lt > 0 && gt > lt)
        {
            var inner = type.Substring(lt + 1, gt - lt - 1);
            return inner.Split(',')[0].Trim();
        }
        if (type.EndsWith("[]")) return type[..^2];
        return null;
    }
    protected static string? ExtractDictionaryValueType(string type)
    {
        if (!IsDictionaryType(type)) return null;
        var inner = type["Dictionary<".Length..];
        inner = inner.TrimEnd('>');
        var parts = inner.Split(',');
        if (parts.Length == 2) return parts[1].Trim();
        return null;
    }
    #endregion
}
