using CodeGenerator.Generate.LanguageFormatter;

namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// 抽取自 RequestGenerate 的公共部分, 专注于解析 OpenApi 并生成模型与函数元数据。
/// 子类只需实现各自的服务文件生成逻辑。
/// </summary>
public abstract class ClientRequestBase(OpenApiDocument openApi)
{
    protected abstract LanguageFormatterBase Formatter { get; }

    protected OpenApiPaths PathsPairs { get; } = openApi.Paths;
    protected ISet<OpenApiTag>? ApiTags { get; } = openApi.Tags;
    public IDictionary<string, IOpenApiSchema>? Schemas { get; set; } = openApi.Components?.Schemas;
    public OpenApiDocument OpenApi { get; set; } = openApi;

    /// <summary>模型文件集合</summary>
    public List<GenFileInfo> ModelFiles { get; } = [];

    /// <summary>已解析的 Schema 元信息集合</summary>
    public List<TypeMeta> SchemaMetas { get; } = [];

    /// <summary>已解析的请求函数集合</summary>
    public List<RequestServiceFunction> RequestFunctions { get; } = [];

    /// <summary>枚举类型集合 (名称)</summary>
    public List<string> EnumModels { get; } = [];

    protected abstract List<GenFileInfo> InternalBuildServices(ISet<OpenApiTag> tags, string docName, List<RequestServiceFunction> functions);

    /// <summary>
    /// 解析全部 Schemas 并返回元信息集合 (不生成文件)。
    /// </summary>
    public List<TypeMeta> ParseSchemas()
    {
        SchemaMetas.Clear();
        if (Schemas == null)
        {
            return SchemaMetas;
        }
        int enumCount = 0;
        foreach (var kv in Schemas)
        {
            var meta = OpenApiHelper.ParseSchemaToTypeMeta(kv.Key, kv.Value);
            SchemaMetas.Add(meta);
            if (meta.IsEnum == true) enumCount++;
        }
        Console.WriteLine($"[ParseSchemas] Total: {SchemaMetas.Count}, Enum: {enumCount}");
        return SchemaMetas;
    }

    /// <summary>
    /// 根据已解析的 SchemaMetas 生成模型文件集合。
    /// </summary>
    public abstract List<GenFileInfo> GenerateModelFiles();

    /// <summary>
    /// 使用标准 C# 映射与 Formatter 格式化器生成目标语言类型
    /// </summary>
    protected string GetLanguageType(IOpenApiSchema? schema)
    {
        if (schema == null) return string.Empty;
        bool isEnum = (schema.Enum?.Count ?? 0) > 0 || schema.Extensions?.ContainsKey("x-enumData") == true;
        bool isList = schema.Type == JsonSchemaType.Array;
        bool isNullable = schema.Type.HasValue && schema.Type.Value.HasFlag(JsonSchemaType.Null);

        string csharpType = OpenApiHelper.MapToCSharpType(schema);
        if (schema is OpenApiSchemaReference r && r.Reference.Id is not null)
        {
            csharpType = r.Reference.Id;
        }
        // 处理泛型类型: e.g. PageList`1 需要转换为 PageList<T> 或具体类型
        if (csharpType.Contains('`'))
        {
            csharpType = OpenApiHelper.ParseGenericFullName(csharpType);
        }
        return Formatter.FormatType(csharpType, isEnum, isList, isNullable);
    }
    public List<RequestServiceFunction> ParseOperations()
    {
        RequestFunctions.Clear();
        foreach (var path in PathsPairs)
        {
            if (path.Value.Operations == null || path.Value.Operations.Count == 0) continue;
            foreach (var operation in path.Value.Operations)
            {
                RequestServiceFunction function = new()
                {
                    Description = operation.Value?.Summary,
                    Method = operation.Key.ToString(),
                    Name = operation.Value?.OperationId ?? (operation.Key.ToString() + path.Key),
                    Path = path.Key,
                    Tag = operation.Value?.Tags?.FirstOrDefault()?.Name,
                };
                if (string.IsNullOrWhiteSpace(function.Name))
                {
                    function.Name = operation.Key + "_" + path.Key.Split('/').LastOrDefault();
                }
                var reqSchema = operation.Value?.RequestBody?.Content?.Values.FirstOrDefault()?.Schema;
                IOpenApiSchema? respSchema = null;
                var responses = operation.Value?.Responses;
                if (responses != null && responses.Count > 0)
                {
                    IOpenApiResponse? selectedResponse = null;
                    if (responses.Count == 1)
                    {
                        selectedResponse = responses.Values.First();
                    }
                    else
                    {
                        if (responses.ContainsKey("200"))
                        {
                            selectedResponse = responses["200"];
                        }
                        else
                        {
                            selectedResponse = responses.Values.First();
                        }
                    }
                    respSchema = selectedResponse?.Content?.FirstOrDefault().Value?.Schema;
                }
                function.RequestRefType = OpenApiHelper.GetRootRef(reqSchema);
                function.ResponseRefType = OpenApiHelper.GetRootRef(respSchema);
                function.RequestType = GetLanguageType(reqSchema);
                function.ResponseType = GetLanguageType(respSchema);
                function.Params = operation.Value?.Parameters?.Select(p =>
                {
                    string? location = p.In?.GetDisplayName();
                    bool? inpath = location?.ToLower()?.Equals("path");
                    string type = GetLanguageType(p.Schema);
                    string? refType = OpenApiHelper.GetRootRef(p.Schema);
                    return new FunctionParams
                    {
                        Description = p.Description,
                        RefType = refType,
                        Name = p.Name,
                        InPath = inpath ?? false,
                        IsRequired = p.Required,
                        Type = type,
                    };
                }).ToList();
                // 处理相同方法，不同请求谓词的情况
                if (RequestFunctions.Any(f => f.Name == function.Name))
                {
                    function.Name = function.Name + operation.Key.ToString();
                }

                RequestFunctions.Add(function);
            }
        }

        Console.WriteLine($"[ParseRoute]: {RequestFunctions.Count}");
        return RequestFunctions;
    }

    /// <summary>
    /// 生成服务文件集合 (由子类实现具体逻辑)。
    /// 自动确保先解析 Schemas 与 Operations。
    /// </summary>
    public List<GenFileInfo> GenerateServices(ISet<OpenApiTag> tags, string docName)
    {
        if (SchemaMetas.Count == 0) ParseSchemas();
        if (ModelFiles.Count == 0) GenerateModelFiles();
        if (RequestFunctions.Count == 0) ParseOperations();
        return InternalBuildServices(tags, docName, RequestFunctions);
    }
}

public record FunctionBuildResult(string Name, string ParamsString, string Comments, string DataString, string Path)
{
    public string ResponseType { get; set; } = string.Empty;
}