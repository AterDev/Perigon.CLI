using System.Text;
using CodeGenerator.Models;

namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// TypeScript 格式化以及模型代码生成。
/// </summary>
public class TypeScriptFormatter : LanguageFormatter
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

    public override string FormatType(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false)
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

    public override string GenerateModel(TypeMeta meta)
    {
        if (meta.IsEnum == true)
        {
            return GenerateEnum(meta);
        }
        return GenerateInterface(meta);
    }


    private string GenerateEnum(TypeMeta meta)
    {
        var cw = new Helper.CodeWriter();
        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/**")
              .AppendLine(" * " + meta.Comment)
              .AppendLine(" */");
        }
        cw.OpenBlock($"export enum {FormatSchemaKey(meta.Name)}");
        foreach (var p in meta.PropertyInfos)
        {
            cw.AppendLine($"/** {p.CommentSummary} */");
            cw.AppendLine($"{p.Name} = {p.DefaultValue},");
        }
        cw.CloseBlock();
        return cw.ToString();
    }
    private string GenerateInterface(TypeMeta meta)
    {
        var importRefs = new HashSet<(string Ref, bool IsEnum)>();
        var sbProps = new StringBuilder();

        // 泛型参数实际类型 -> 占位符 映射
        var genericMap = new Dictionary<string, string>();
        if (meta.IsGeneric && meta.GenericParams.Count > 0)
        {
            for (int i = 0; i < meta.GenericParams.Count; i++)
            {
                var gp = meta.GenericParams.ElementAt(i);
                // 使用统一的占位符命名: T1, T2, ... 以便与前端预期一致
                string placeholder = $"T{i + 1}";
                genericMap[OpenApiHelper.FormatSchemaKey(gp.Name)] = placeholder;
            }
        }

        foreach (var p in meta.PropertyInfos)
        {
            bool isNullable = p.IsNullable;
            if (meta.Name.EndsWith(Entity.ConstVal.FilterDto, StringComparison.OrdinalIgnoreCase) ||
                meta.Name.EndsWith(Entity.ConstVal.UpdateDto, StringComparison.OrdinalIgnoreCase))
            {
                isNullable = true;
            }
            string tsPropType = p.Type;
            bool replacedByGeneric = false;
            if (!string.IsNullOrWhiteSpace(tsPropType) && genericMap.Count > 0)
            {
                // 提取元素类型 (处理 数组 / List<> / IEnumerable<>)
                string rawType = tsPropType;
                bool isArray = rawType.EndsWith("[]");
                string elementType;
                if (isArray)
                {
                    elementType = rawType[..^2];
                }
                else if (p.IsList || rawType.StartsWith("List<") || rawType.StartsWith("IEnumerable<") || rawType.StartsWith("ICollection<") || rawType.StartsWith("IList<"))
                {
                    elementType = ExtractGenericArgument(rawType) ?? rawType;
                }
                else
                {
                    elementType = rawType;
                }

                var formattedElementType = OpenApiHelper.FormatSchemaKey(elementType);
                if (genericMap.TryGetValue(formattedElementType, out var placeholder))
                {
                    tsPropType = placeholder + ((isArray || p.IsList) ? "[]" : string.Empty);
                    replacedByGeneric = true;
                }
            }
            sbProps.Append(FormatProperty(p.Name, tsPropType, p.IsEnum, p.IsList, isNullable, p.CommentSummary));
            var reference = p.NavigationName ?? string.Empty;
            if (!replacedByGeneric && !string.IsNullOrWhiteSpace(reference) && reference != meta.FullName)
            {
                importRefs.Add((reference, p.IsEnum));
            }
        }

        var cw = new Helper.CodeWriter();
        var distinctImports = importRefs.Distinct().ToList();
        foreach (var (refTypeFullName, isEnum) in distinctImports)
        {
            var refType = FormatSchemaKey(refTypeFullName);
            var nsName = OpenApiHelper.GetNamespace(refTypeFullName);
            var nsParts = OpenApiHelper.GetNamespaceFirstPart(nsName);
            cw.AppendLine($"import {{ {refType} }} from '../{nsParts.ToHyphen()}/{refType.ToHyphen()}.model';");
        }
        if (distinctImports.Count > 0) cw.AppendLine();

        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/**")
              .AppendLine(" * " + meta.Comment)
              .AppendLine(" */");
        }

        var baseName = FormatSchemaKey(meta.Name.Split('<')[0]);
        if (meta.IsGeneric && genericMap.Count > 0)
        {
            baseName += "<" + string.Join(",", genericMap.Values) + ">";
        }
        cw.OpenBlock($"export interface {baseName}");
        foreach (var line in sbProps.ToString().Split('\n'))
        {
            if (!string.IsNullOrWhiteSpace(line)) cw.AppendLine(line);
        }
        cw.CloseBlock();
        return cw.ToString();
    }

    private string FormatProperty(string name, string csharpType, bool isEnum, bool isList, bool isNullable, string? comment)
    {
        var tsType = FormatType(csharpType ?? "any", isEnum, isList, isNullable);
        string propName = name + (isNullable ? "?: " : ": ");
        string comments = $"/** {(!string.IsNullOrWhiteSpace(comment) ? comment : name)} */";
        return $"{comments}\n{propName}{tsType};\n";
    }

    #region helpers
    private static string Normalize(string csharpType)
    {
        if (string.IsNullOrWhiteSpace(csharpType)) return "any";
        var cleaned = StripGenericArity(csharpType);

        if (cleaned.IndexOf('.') > 0)
        {
            cleaned = cleaned.Split('.').Last();
        }

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
    #endregion
}
