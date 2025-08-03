namespace CodeGenerator.Generate;

/// <summary>
/// generate csharp model file
/// </summary>
public class CsharpModelGenerate : GenerateBase
{
    public static List<string> EnumModels { get; set; } = [];
    public Dictionary<string, string?> ModelDictionary { get; set; } = [];

    public CsharpModelGenerate(OpenApiDocument openApi)
    {
        foreach (KeyValuePair<string, IOpenApiPathItem> path in openApi.Paths)
        {
            if (path.Value.Operations == null)
                continue;

            foreach (var operation in path.Value.Operations)
            {
                string? tag = operation.Value.Tags?.FirstOrDefault()?.Name;
                IOpenApiSchema? requestSchema = operation
                    .Value.RequestBody?.Content?.Values.FirstOrDefault()
                    ?.Schema;
                IOpenApiSchema? responseSchema = operation
                    .Value.Responses?.FirstOrDefault()
                    .Value?.Content?.FirstOrDefault()
                    .Value?.Schema;
                (string? RequestType, string? requestRefType) = OpenApiHelper.ParseParamAsCSharp(
                    requestSchema
                );
                (string? ResponseType, string? responseRefType) = OpenApiHelper.ParseParamAsCSharp(
                    responseSchema
                );

                // 存储对应的Tag
                // 请求dto
                if (requestRefType != null && !string.IsNullOrEmpty(requestRefType))
                {
                    _ = ModelDictionary.TryAdd(requestRefType, tag);
                }
                // 返回dto
                if (responseRefType != null && !string.IsNullOrEmpty(responseRefType))
                {
                    _ = ModelDictionary.TryAdd(responseRefType, tag);
                }

                Dictionary<string, string?>? relationModels = GetRelationModels(requestSchema, tag);
                if (relationModels != null)
                {
                    foreach (KeyValuePair<string, string?> item in relationModels)
                    {
                        _ = ModelDictionary.TryAdd(item.Key, item.Value);
                    }
                }
                relationModels = GetRelationModels(responseSchema, tag);
                if (relationModels != null)
                {
                    foreach (KeyValuePair<string, string?> item in relationModels)
                    {
                        _ = ModelDictionary.TryAdd(item.Key, item.Value);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 获取相关联的模型
    /// </summary>
    public Dictionary<string, string?>? GetRelationModels(IOpenApiSchema? schema, string? tag = "")
    {
        if (schema == null)
        {
            return default;
        }
        Dictionary<string, string?> dic = [];
        // 父类
        if (schema.AllOf != null)
        {
            var parent = schema.AllOf.FirstOrDefault();
            if (parent != null && parent is OpenApiSchemaReference reference)
            {
                if (reference.Reference.Id != null)
                {
                    if (!dic.ContainsKey(reference.Reference.Id))
                    {
                        dic.Add(reference.Reference.Id, null);
                    }
                }
            }
        }
        // 属性中的类型
        var props = schema
            .Properties?.Where(p => p.Value.OneOf != null && p.Value.OneOf.Count > 0)
            .Select(s => s.Value)
            .ToList();
        if (props != null)
        {
            foreach (var prop in props)
            {
                if (
                    prop.OneOf?.Count > 0
                    && prop.OneOf.FirstOrDefault() is OpenApiSchemaReference reference
                )
                {
                    var refId = reference.Reference.Id ?? "";
                    dic.TryAdd(refId, tag);
                }
            }
        }
        // 数组
        var arr = schema
            .Properties?.Where(p => p.Value.Type == JsonSchemaType.Array)
            .Select(s => s.Value)
            .ToList();
        if (arr != null)
        {
            foreach (var item in arr)
            {
                if (
                    item.Items?.OneOf != null
                    && item.Items.OneOf.Count > 0
                    && item.OneOf?.FirstOrDefault() is OpenApiSchemaReference reference
                )
                {
                    var refId = reference.Reference.Id ?? "";
                    dic.TryAdd(refId, tag);
                }
            }
        }
        return dic;
    }

    /// <summary>
    /// 生成模型类文件
    /// </summary>
    public GenFileInfo GenerateModelFile(string schemaKey, IOpenApiSchema schema, string nspName)
    {
        string fileName = schemaKey.ToPascalCase() + ".cs";
        string modelContent;
        string? dirName = GetDirName(schemaKey);
        string path = Path.Combine("Models", dirName ?? "");
        if (schema.Enum?.Count > 0)
        {
            modelContent = ToEnumString(schema, schemaKey, nspName);
            EnumModels.Add(schemaKey);
        }
        else
        {
            modelContent = ToClassModelString(schema, schemaKey, nspName);
        }
        GenFileInfo file = new(fileName, modelContent)
        {
            FullName = path ?? "",
            Content = modelContent,
            ModelName = schemaKey,
        };
        return file;
    }

    private string? GetDirName(string searchKey)
    {
        return ModelDictionary
            .Where(m => m.Key.StartsWith(searchKey))
            .Select(m => m.Value)
            .FirstOrDefault();
    }

    /// <summary>
    /// 将 Schemas 转换成 csharp class
    /// </summary>
    public string ToClassModelString(IOpenApiSchema schema, string name = "", string nspName = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        string extendString = "";
        string importString = "";
        if (string.IsNullOrEmpty(GetDirName(name)))
        {
            // TODO:
        }
        if (
            schema.AllOf?.Count > 0
            && schema.AllOf.FirstOrDefault() is OpenApiSchemaReference reference
        )
        {
            string? extend = reference.Reference.Id;
            if (!string.IsNullOrEmpty(extend))
            {
                extendString = " : " + extend + "";
                if (extend != name)
                {
                    string? dirName = GetDirName(name);
                    dirName = dirName.NotEmpty() ? dirName!.ToHyphen() + "/" : "";
                    importString += @$"using {nspName}.Models;" + Environment.NewLine;
                }
            }
        }
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                $"""
                    /// <summary>
                    /// {schema.Description}
                    /// </summary>
                    """ + Environment.NewLine;
        }
        var props = OpenApiHelper.ParseProperties(schema, false);
        bool preferenceNull = name.EndsWith("FilterDto") || name.EndsWith("UpdateDto");
        foreach (var p in props)
        {
            propertyString += CSProperty.ToProperty(
                p.Name,
                p.Type,
                p.IsNullable,
                p.CommentSummary,
                p.Type,
                p.IsEnum,
                p.IsList,
                preferenceNull
            );
        }
        string namespaceString = $"namespace {nspName}.Models;" + Environment.NewLine;
        res =
            @$"{importString}{namespaceString}{comment}public class {name} {extendString}{{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 生成enum
    /// </summary>
    public static string ToEnumString(IOpenApiSchema schema, string name = "", string nspName = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                $"""
                    /// <summary>
                    /// {schema.Description.ReplaceLineEndings("")}
                    /// </summary>
                    """ + Environment.NewLine;
        }
        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        foreach (var prop in enumProps)
        {
            propertyString += $"""
                    [Description(\"{prop.CommentSummary}\")]
                    {prop.Name} = {prop.DefaultValue},

                """;
        }
        string namespaceString = $"namespace {nspName}.Models;" + Environment.NewLine;
        res =
            @$"using System.ComponentModel;
{namespaceString}{comment}public enum {name} {{
{propertyString}
}}
";
        return res;
    }
}

public class CSProperty
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? RefType { get; set; }
    public bool IsEnum { get; set; }
    public bool IsNullable { get; set; }
    public bool IsList { get; set; }
    public string? Comments { get; set; }
    public string? Description { get; set; }

    public static string ToProperty(
        string? name,
        string? type,
        bool isNullable,
        string? description,
        string? refType,
        bool isEnum,
        bool isList,
        bool preferenceNull = false
    )
    {
        if (preferenceNull && !string.IsNullOrWhiteSpace(refType))
        {
            isNullable = true;
        }
        string typeStr = type + (isNullable ? "?" : "");
        string defaultValue = string.Empty;
        if (!isNullable && !isEnum && !isList)
        {
            defaultValue = " = default!;";
        }
        if (isList && string.IsNullOrEmpty(defaultValue))
        {
            defaultValue = " = [];";
        }
        string comments = string.IsNullOrEmpty(description)
            ? string.Empty
            : $"    /// <summary>\n    /// {description}\n    /// </summary>\n";
        return $"{comments}    public {typeStr} {name?.ToPascalCase()} {{ get; set; }}{defaultValue}"
            + Environment.NewLine;
    }
}
