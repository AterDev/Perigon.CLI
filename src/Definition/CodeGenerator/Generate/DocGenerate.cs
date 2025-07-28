namespace CodeGenerator.Generate;

public class DocGenerate(IDictionary<string, IOpenApiSchema> schemas) : GenerateBase
{
    public IDictionary<string, IOpenApiSchema> Schemas { get; set; } = schemas;
    public List<OpenApiTag>? ApiTags { get; set; }

    public void SetTags(List<OpenApiTag> apiTags)
    {
        ApiTags = apiTags;
    }

    public string GetMarkdownContent()
    {
        string docContent = "";
        string itemContent = "";
        string tocContent = "- [目录](#目录)" + Environment.NewLine;

        Schemas = Schemas.OrderBy(s => s.Key).ToDictionary(s => s.Key, s => s.Value);

        foreach (var schema in Schemas)
        {
            var description =
                schema.Value.AllOf?.LastOrDefault()?.Description ?? schema.Value.Description;

            description = description?.Replace("\n", " ") ?? "";
            // 构建目录

            string des = string.IsNullOrEmpty(description) ? "" : "-" + description;
            des = des.Replace(" ", "-").Replace(" = ", "=").Replace(",", "");

            if (!string.IsNullOrEmpty(description))
            {
                description = $"({description})".Replace(" = ", "=");
            }

            string toc =
                $"\t- [{schema.Key} {description}](#{schema.Key.ToLower()}{des})"
                + Environment.NewLine;

            tocContent += toc;

            string header =
                $"### [{schema.Key}](#{schema.Key}) {description}" + Environment.NewLine;

            var props = OpenApiHelper.ParseProperties(schema.Value);

            string row = "|字段名|类型|必须|说明|" + Environment.NewLine;
            row += "|-|-|-|-|" + Environment.NewLine;
            props.ForEach(p =>
            {
                row += FormatProperty(p);
            });
            itemContent += header + row + Environment.NewLine;
        }
        docContent = tocContent + itemContent;
        return docContent;
    }

    public static string FormatProperty(PropertyInfo property)
    {
        string? comments = property
            .CommentXml?.Replace("*", "")
            .Replace("/", "")
            .Replace("\r\n", Environment.NewLine)
            .Replace("\n\r", Environment.NewLine)
            .Replace("\r", Environment.NewLine)
            .Replace(Environment.NewLine + "*", "")
            .Replace(Environment.NewLine, "")
            .Replace("\n", ";")
            .Replace(" ", "")
            .Trim([';']);

        if (string.IsNullOrEmpty(comments))
        {
            comments = "-";
        }

        string? type = property.Type?.Replace(" | null", "");
        string isMust = property.IsNullable == false ? "false" : "true";

        return $"|{property.Name}" + $"|{type}|{isMust}|{comments}|" + Environment.NewLine;
    }
}
