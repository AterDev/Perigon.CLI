namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// C# 格式化以及模型代码生成。
/// </summary>
public class CSharpFormatter : LanguageFormatterBase
{
    public override string FormatType(
        string csharpType,
        bool isEnum = false,
        bool isList = false,
        bool isNullable = false
    )
    {
        if (string.IsNullOrWhiteSpace(csharpType)) return "object";

        // C# 类型通常不需要像 TS 那样进行复杂的映射，直接使用即可
        // 但需要处理可空和集合
        string type = csharpType;

        if (isList && !type.StartsWith("List<") && !type.EndsWith("[]"))
        {
            type = $"List<{type}>";
        }

        if (isNullable && !type.EndsWith("?"))
        {
            // 引用类型和值类型都加 ? 以支持可空引用类型
            type += "?";
        }

        return type;
    }

    public override string GenerateModel(TypeMeta meta, string projectName = "")
    {
        var nspName = $"{projectName}.Models.{OpenApiHelper.GetNamespaceFirstPart(meta.Namespace)}";

        return meta.IsEnum == true ? GenerateEnum(meta, nspName) : GenerateClass(meta, nspName);
    }

    private string GenerateEnum(TypeMeta meta, string nspName = "")
    {
        var cw = new CodeWriter();
        cw.AppendLine("using System.ComponentModel;");
        cw.AppendLine($"namespace {nspName};");
        cw.AppendLine();

        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/// <summary>")
              .AppendLine($"/// {meta.Comment.ReplaceLineEndings("")}")
              .AppendLine("/// </summary>");
        }

        cw.OpenBlock($"public enum {FormatSchemaKey(meta.Name)}");
        foreach (var p in meta.PropertyInfos)
        {
            cw.AppendLine($"[Description(\"{p.CommentSummary}\")]");
            cw.AppendLine($"{p.Name} = {p.DefaultValue},");
            cw.AppendLine();
        }
        cw.CloseBlock();
        return cw.ToString();
    }

    private string GenerateClass(TypeMeta meta, string nspName)
    {
        var importRefs = new HashSet<string>();
        var cw         = new CodeWriter(4);

        var genericMap = new Dictionary<string, string>();
        if (meta.IsGeneric && meta.GenericParams.Count > 0)
        {
            for (int i = 0; i < meta.GenericParams.Count; i++)
            {
                var gp = meta.GenericParams.ElementAt(i);
                string placeholder = $"T{i + 1}";
                genericMap[OpenApiHelper.FormatSchemaKey(gp.Name)] = placeholder;
            }
        }

        // 收集引用命名空间
        cw.AppendLine($"namespace {nspName};");
        cw.AppendLine();

        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/// <summary>")
              .AppendLine($"/// {meta.Comment}")
              .AppendLine("/// </summary>");
        }

        var modelName = FormatSchemaKey(meta.Name);
        if (meta.IsGeneric && genericMap.Count > 0)
        {
            modelName += "<" + string.Join(",", genericMap.Values) + ">";
        }
        string classDecl = $"public class {modelName}";
        cw.OpenBlock(classDecl, true);

        foreach (var property in meta.PropertyInfos)
        {
            string propType = property.Type;
            if (property.IsNullable && !propType.EndsWith("?"))
            {
                propType += "?";
            }
            // 泛型处理
            if (!string.IsNullOrWhiteSpace(propType) && genericMap.Count > 0)
            {
                string rawType = propType;
                bool isArray = rawType.EndsWith("[]");
                string elementType = isArray
                    ? rawType[..^2]
                    : property.IsList || rawType.StartsWith("List<") || rawType.StartsWith("IEnumerable<") || rawType.StartsWith("ICollection<") || rawType.StartsWith("IList<")
                        ? ExtractGenericArgument(rawType) ?? rawType
                        : rawType;
                var formattedElementType = OpenApiHelper.FormatSchemaKey(elementType);
                if (genericMap.TryGetValue(formattedElementType, out var placeholder))
                {
                    propType = ((isArray || property.IsList) ? $"List<{placeholder}>" : placeholder);
                }
            }

            // 处理 List 初始化
            string defaultValue = string.Empty;
            if (!property.IsNullable && !property.IsEnum && !property.IsList)
            {
                defaultValue = " = default!;";
            }
            if (property.IsList)
            {
                defaultValue = " = [];";
            }

            if (!string.IsNullOrWhiteSpace(property.CommentSummary))
            {
                cw.AppendLine("/// <summary>")
                  .AppendLine($"/// {property.CommentSummary}")
                  .AppendLine("/// </summary>");
            }

            cw.AppendLine($"public {propType} {property.Name.ToPascalCase()} {{ get; set; }}{defaultValue}");
        }

        cw.CloseBlock();
        return cw.ToString();
    }
}
