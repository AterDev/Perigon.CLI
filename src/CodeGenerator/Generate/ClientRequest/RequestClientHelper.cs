using Share.Helper;

namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// 提供与请求客户端生成相关的静态辅助方法: 基础服务模板、枚举Pipe、枚举函数等。
/// 原先位于 RequestGenerate 中, 已拆分实现类后保留静态方法供调用方使用。
/// </summary>
public static class RequestClientHelper
{
    private static readonly HashSet<string> CSharpKeywords =
    [
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
        "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else", "enum",
        "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach", "goto",
        "if", "implicit", "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
        "new", "null", "object", "operator", "out", "override", "params", "private", "protected", "public",
        "readonly", "record", "ref", "return", "sbyte", "sealed", "short", "sizeof", "stackalloc", "static",
        "string", "struct", "switch", "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while"
    ];

    /// <summary>
    /// 获取基础服务模板内容
    /// </summary>
    public static string GetBaseService(RequestClientType libType)
    {
        try
        {
            return libType switch
            {
                RequestClientType.NgHttp => GenerateBase.GetTplContent("angular.base.service.tpl"),
                RequestClientType.Axios => GenerateBase.GetTplContent("RequestService.axios.service.tpl"),
                _ => string.Empty,
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine("request base service:" + ex.Message + ex.StackTrace + ex.InnerException);
            return string.Empty;
        }
    }

    public static string GetEnumPipeContent(IDictionary<string, IOpenApiSchema> schemas, bool isNgModule = false)
    {
        var codeWriter = new CodeWriter();

        foreach (var item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                var switchCode = ToEnumSwitchString(OpenApiHelper.FormatSchemaKey(item.Key), item.Value);
                codeWriter.AppendLine(switchCode);
            }
        }

        return TplContent.EnumPipeTpl(codeWriter.ToString(), isNgModule);
    }

    public static string ToEnumSwitchString(string enumType, IOpenApiSchema schema)
    {
        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        if (enumProps == null || enumProps.Count == 0) return string.Empty;

        var codeWriter = new CodeWriter();
        codeWriter.Indent().Indent().Indent();
        codeWriter.AppendLine($"case '{enumType}':");
        codeWriter.Indent();
        codeWriter.AppendLine("switch (value) {");
        codeWriter.Indent();

        foreach (var prop in enumProps)
        {
            codeWriter.AppendLine($"case {prop.DefaultValue}: result = '{prop.CommentSummary}'; break;");
        }

        codeWriter.AppendLine("default: result = '默认'; break;");
        codeWriter.Unindent();
        codeWriter.AppendLine("}");
        codeWriter.AppendLine("break;");

        return codeWriter.ToString();
    }

    /// <summary>
    /// 将 OpenAPI 参数名转换为代码中可安全使用的小驼峰变量名。
    /// </summary>
    public static string NormalizeParameterName(string? rawName, ISet<string>? usedNames = null)
    {
        var baseName = ToCamelCaseInternal(rawName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(baseName))
        {
            baseName = "value";
        }

        if (!char.IsLetter(baseName[0]) && baseName[0] != '_')
        {
            baseName = $"arg{ToUpperFirstInternal(baseName)}";
        }

        if (CSharpKeywords.Contains(baseName))
        {
            baseName += "Value";
        }

        if (usedNames == null)
        {
            return baseName;
        }

        var candidate = baseName;
        var index = 2;
        while (!usedNames.Add(candidate))
        {
            candidate = $"{baseName}{index}";
            index++;
        }

        return candidate;
    }

    /// <summary>
    /// 将 OpenAPI tag 转换为可用于 C# 和 TypeScript 服务类的 PascalCase 标识符。
    /// </summary>
    public static string NormalizeServiceName(string? rawName)
    {
        var name = ToPascalCaseInternal(rawName ?? string.Empty);
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Api";
        }

        if (!char.IsLetter(name[0]) && name[0] != '_')
        {
            name = "Api" + name;
        }

        return name;
    }

    private static string ToCamelCaseInternal(string value)
    {
        var pascal = ToPascalCaseInternal(value);
        if (string.IsNullOrWhiteSpace(pascal))
        {
            return string.Empty;
        }
        return char.ToLowerInvariant(pascal[0]) + pascal[1..];
    }

    private static string ToPascalCaseInternal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var parts = value
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : ' ')
            .ToArray();

        return string.Join(
            string.Empty,
            new string(parts)
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(ToUpperFirstInternal)
        );
    }

    private static string ToUpperFirstInternal(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return char.ToUpperInvariant(value[0]) + value[1..];
    }
}
