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
        var cw = new Helper.TsCodeWriter();
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

        // 根据属性的类型与泛型占位符进行匹配，不再固定字段名。
        // 规则：
        // 1. 仅在 meta.IsGeneric 时尝试替换。
        // 2. 找出所有引用其他模型(非原始/非枚举)的属性，按出现顺序与 GenericParams 对应。
        // 3. 若属性是列表，替换为 T[] / T1[]...
        // 4. 若属性是单值，替换为 T / T1 ...
        // 5. 被替换的属性不再加入 importRefs。
        var genericReplacementTargets = new List<(PropertyInfo Prop, string Placeholder)>();
        if (meta.IsGeneric && meta.GenericParams.Count > 0)
        {
            var gpList = meta.GenericParams.ToList();
            int gpIndex = 0;
            foreach (var prop in meta.PropertyInfos)
            {
                if (gpIndex >= gpList.Count) break;
                if (prop.IsEnum) continue; // 枚举不参与泛型替换
                var baseType = prop.Type.TrimEnd();
                bool isPrimitive = PrimitiveMap.ContainsKey(baseType) || baseType.Equals("Enum(int)");
                if (isPrimitive) continue;
                if (OpenApiHelper.FormatSchemaKey(baseType) == FormatSchemaKey(meta.Name)) continue;
                // 将该属性映射到对应泛型参数
                genericReplacementTargets.Add((prop, gpList[gpIndex].Name));
                gpIndex++;
            }
        }

        foreach (var p in meta.PropertyInfos)
        {
            bool isNullable = p.IsNullable;
            // FilterDto / UpdateDto 特殊可空规则
            if (meta.Name.EndsWith(Entity.ConstVal.FilterDto, StringComparison.OrdinalIgnoreCase) ||
                meta.Name.EndsWith(Entity.ConstVal.UpdateDto, StringComparison.OrdinalIgnoreCase))
            {
                isNullable = true;
            }
            var tsPropType = p.Type;
            var replacement = genericReplacementTargets.FirstOrDefault(t => t.Prop == p);
            if (!string.IsNullOrEmpty(replacement.Placeholder))
            {
                if (p.IsList || tsPropType.EndsWith("[]"))
                {
                    tsPropType = replacement.Placeholder + "[]";
                }
                else
                {
                    tsPropType = replacement.Placeholder;
                }
            }
            sbProps.Append(FormatProperty(p.Name, tsPropType, p.IsEnum, p.IsList, isNullable, p.CommentSummary));
            var reference = p.NavigationName ?? string.Empty;
            // 若已替换为泛型占位，则不导入原始引用类型
            bool replaced = !string.IsNullOrEmpty(replacement.Placeholder);
            if (!replaced && !string.IsNullOrWhiteSpace(reference) && reference != meta.Name)
            {
                importRefs.Add((reference, p.IsEnum));
            }
        }

        var cw = new Helper.TsCodeWriter();
        // imports - 排除自身引用 (如属性 children: SystemOrganization[] 不应 import 自己)
        var selfName = FormatSchemaKey(meta.Name);
        var distinctImports = importRefs
            .Where(r => !string.Equals(FormatSchemaKey(r.Ref), selfName, StringComparison.Ordinal))
            .Distinct()
            .ToList();
        foreach (var (Ref, IsEnum) in distinctImports)
        {
            var refType = FormatSchemaKey(Ref);
            if (string.Equals(refType, selfName, StringComparison.Ordinal)) continue; // 双重保险
            string dirName = string.Empty;
            string relatePath = "./";
            if (IsEnum)
            {
                relatePath = "../";
                dirName = "enum/";
            }
            cw.AppendLine($"import {{ {refType} }} from '{relatePath}{dirName}{refType.ToHyphen()}.model';");
        }
        if (distinctImports.Count > 0) cw.AppendLine();

        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/**")
              .AppendLine(" * " + meta.Comment)
              .AppendLine(" */");
        }
        var ifaceName = FormatSchemaKey(meta.Name);
        if (meta.IsGeneric)
        {
            // 使用占位泛型参数 (T, T1, T2...) 与 TypeMeta.GenericParams 顺序对应
            var gpList = meta.GenericParams.Select(g => g.Name).ToList();
            if (gpList.Count > 0)
            {
                ifaceName += "<" + string.Join(",", gpList) + ">";
            }
        }
        cw.OpenBlock($"export interface {ifaceName}");
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
