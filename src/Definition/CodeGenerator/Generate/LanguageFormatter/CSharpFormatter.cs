namespace CodeGenerator.Generate.LanguageFormatter;

/// <summary>
/// C# 格式化以及模型代码生成。
/// </summary>
public class CSharpFormatter : LanguageFormatterBase
{
    public override string FormatType(string csharpType, bool isEnum = false, bool isList = false, bool isNullable = false)
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

    public override string GenerateModel(TypeMeta meta)
    {
        return meta.IsEnum == true ? GenerateEnum(meta) : GenerateClass(meta);
    }

    private string GenerateEnum(TypeMeta meta)
    {
        var cw = new CodeWriter();
        cw.AppendLine("using System.ComponentModel;");
        cw.AppendLine($"namespace {meta.Namespace};");
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

    private string GenerateClass(TypeMeta meta)
    {
        var cw = new CodeWriter();
        // 收集引用命名空间
        var imports = new HashSet<string>();


        cw.AppendLine($"namespace {meta.Namespace};");
        cw.AppendLine();

        if (!string.IsNullOrWhiteSpace(meta.Comment))
        {
            cw.AppendLine("/// <summary>")
              .AppendLine($"/// {meta.Comment}")
              .AppendLine("/// </summary>");
        }

        string classDecl = $"public class {FormatSchemaKey(meta.Name)}";


        cw.OpenBlock(classDecl);

        foreach (var property in meta.PropertyInfos)
        {
            string typeStr = property.Type;
            if (property.IsNullable && !typeStr.EndsWith("?"))
            {
                typeStr += "?";
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

            cw.AppendLine($"public {typeStr} {property.Name.ToPascalCase()} {{ get; set; }}{defaultValue}");
        }

        cw.CloseBlock();
        return cw.ToString();
    }
}
