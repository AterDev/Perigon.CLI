namespace CodeGenerator.Generate;

/// <summary>
/// generate typescript model file
/// </summary>
public class TSModelGenerate : GenerateBase
{
    public Dictionary<string, string?> ModelDictionary { get; set; } = [];

    public OpenApiDocument OpenApi { get; set; }

    public TSModelGenerate(OpenApiDocument openApi)
    {
        OpenApi = openApi;
        foreach (var path in openApi.Paths)
        {
            if (path.Value.Operations == null || path.Value.Operations.Count == 0)
            {
                continue;
            }
            foreach (var operation in path.Value.Operations)
            {
                string? tag = operation.Value?.Tags?.FirstOrDefault()?.Name;

                var requestSchema = operation
                    .Value?.RequestBody?.Content?.Values.FirstOrDefault()
                    ?.Schema;
                var responseSchema = operation
                    .Value?.Responses?.FirstOrDefault()
                    .Value?.Content?.FirstOrDefault()
                    .Value?.Schema;
                (string? RequestType, string? requestRefType) = OpenApiHelper.ParseParamTSType(
                    requestSchema
                );
                (string? ResponseType, string? responseRefType) = OpenApiHelper.ParseParamTSType(
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
    /// <returns></returns>
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
            .Properties?.Where(p => p.Value.OneOf != null)
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
                    if (!dic.ContainsKey(refId))
                    {
                        dic.Add(refId, tag);
                    }
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
            foreach (IOpenApiSchema? item in arr)
            {
                if (
                    item.Items?.OneOf != null
                    && item.Items.OneOf.Count > 0
                    && item.OneOf?.FirstOrDefault() is OpenApiSchemaReference reference
                )
                {
                    var refId = reference.Reference.Id ?? "";
                    if (!dic.ContainsKey(refId))
                    {
                        dic.Add(refId, tag);
                    }
                }
            }
        }

        return dic;
    }

    /// <summary>
    /// 生成ts interface
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GenerateInterfaceFile(string schemaKey, IOpenApiSchema schema)
    {
        // 文件名及内容
        string fileName = schemaKey.ToHyphen() + ".model.ts";
        string tsContent;
        string? path = GetDirName(schemaKey)?.ToHyphen();
        if (schema.Enum?.Count > 0)
        {
            tsContent = ToEnumString(schema, schemaKey);
            path = "enum";
        }
        else
        {
            tsContent = ToInterfaceString(schema, schemaKey);
        }
        GenFileInfo file = new(fileName, tsContent)
        {
            DirName = path ?? "",
            FullName = path ?? "",
            Content = tsContent,
            ModelName = schemaKey,
        };

        return file;
    }

    /// <summary>
    /// 根据类型schema，找到对应所属的目录
    /// </summary>
    /// <param name="searchKey"></param>
    /// <returns></returns>
    private string? GetDirName(string searchKey)
    {
        string? dirName = ModelDictionary
            .Where(m => m.Key.StartsWith(searchKey))
            .Select(m => m.Value)
            .FirstOrDefault();
        return dirName;
    }

    /// <summary>
    /// 将 Schemas 转换成 ts 接口
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <param name="onlyProps"></param>
    /// <returns></returns>
    public string ToInterfaceString(IOpenApiSchema schema, string name = "", bool onlyProps = false)
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        string extendString = "";
        string importString = ""; // 需要导入的关联接口
        string relatePath = "../../";

        // 不在控制器中的类型，则在根目录生成，相对目录也从根目录开始
        if (string.IsNullOrEmpty(GetDirName(name)))
        {
            relatePath = "../";
        }

        if (
            schema.AllOf?.Count > 0
            && schema.AllOf.FirstOrDefault() is OpenApiSchemaReference reference
        )
        {
            string? extend = reference.Reference.Id;
            if (!string.IsNullOrEmpty(extend))
            {
                extendString = "extends " + extend + " ";
                // 如果是自引用，不需要导入
                if (extend != name)
                {
                    string? dirName = GetDirName(name);
                    dirName = dirName.NotEmpty() ? dirName!.ToHyphen() + "/" : "";
                    importString +=
                        @$"import {{ {extend} }} from '{relatePath}models/{extend.ToHyphen()}.model';"
                        + Environment.NewLine;
                }
            }
        }
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                @$"/**
 * {schema.Description}
 */
";
        }
        var props = OpenApiHelper.ParseProperties(schema, false);
        bool preferenceNull = name.EndsWith("FilterDto") || name.EndsWith("UpdateDto");
        var tsProps = new List<TsProperty>();
        foreach (var p in props)
        {
            var tsProperty = new TsProperty
            {
                Type = p.Type,
                Name = p.Name,
                IsEnum = p.IsEnum,
                Reference = p.NavigationName ?? string.Empty,
                IsNullable = p.IsNullable,
                Comments = $"/** {p.CommentSummary} */",
            };
            tsProps.Add(tsProperty);
            propertyString += tsProperty.ToProperty();
        }

        // 去重
        var importsProps = tsProps
            .Where(p => !string.IsNullOrEmpty(p.Reference))
            .DistinctBy(p => p.Reference)
            //.GroupBy(p => p.Reference)
            //.Select(g => new { g.First().IsEnum, g.First().Reference })
            .ToList();
        importsProps.ForEach(ip =>
        {
            // 引用的导入，自引用不需要导入
            if (ip.Reference != name)
            {
                string? dirName = GetDirName(ip.Reference);
                dirName = dirName.NotEmpty() ? dirName!.ToHyphen() + "/" : "";
                if (ip.IsEnum)
                {
                    dirName = "enum/";
                }

                importString +=
                    @$"import {{ {ip.Reference} }} from '{relatePath}{dirName}models/{ip.Reference.ToHyphen()}.model';"
                    + Environment.NewLine;
            }
        });

        res =
            @$"{importString}{comment}export interface {name} {extendString}{{
{propertyString}
}}
";
        return res;
    }

    /// <summary>
    /// 生成enum
    /// </summary>
    /// <param name="schema"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string ToEnumString(IOpenApiSchema schema, string name = "")
    {
        string res = "";
        string comment = "";
        string propertyString = "";
        if (!string.IsNullOrEmpty(schema.Description))
        {
            comment =
                @$"/**
 * {schema.Description}
 */
";
        }
        // 先判断x-enumData
        var enumData = schema.Extensions?.Where(e => e.Key == "x-enumData").FirstOrDefault();

        if (enumData == null)
        {
            return $"{comment}export enum {name} {{}}";
        }

        var enumProps = OpenApiHelper.GetEnumProperties(schema);
        foreach (var item in enumProps)
        {
            propertyString += $"""
                  /** {item.CommentSummary} */
                  {item.Name} = {item.DefaultValue},

                """;
        }

        res =
            @$"{comment}export enum {name} {{
{propertyString}
}}
";
        return res;
    }
}

public class TsProperty
{
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string Reference { get; set; } = string.Empty;
    public bool IsEnum { get; set; }
    public bool IsNullable { get; set; }
    public string? Comments { get; set; }

    public string ToProperty()
    {
        string name = Name + (IsNullable ? "?: " : ": ");
        // 引用的类型可空
        if (!string.IsNullOrEmpty(Reference))
        {
            name = Name + "?: ";
        }

        return $"{Comments}  {name}{Type};" + Environment.NewLine;
    }
}
