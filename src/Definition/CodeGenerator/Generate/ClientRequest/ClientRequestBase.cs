using System.ComponentModel;
using CodeGenerator.Generate.LanguageFormatter;

namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// 抽取自 RequestGenerate 的公共部分, 专注于解析 OpenApi 并生成模型与函数元数据。
/// 子类只需实现各自的服务文件生成逻辑。
/// </summary>
public abstract class ClientRequestBase(OpenApiDocument openApi)
{
    protected static readonly TypeScriptFormatter TsFormatter = new();

    protected OpenApiPaths PathsPairs { get; } = openApi.Paths;
    protected ISet<OpenApiTag>? ApiTags { get; } = openApi.Tags;
    public IDictionary<string, IOpenApiSchema>? Schemas { get; set; } = openApi.Components?.Schemas;
    public OpenApiDocument OpenApi { get; set; } = openApi;

    /// <summary>模型文件集合 (TS)</summary>
    public List<GenFileInfo> TsModelFiles { get; } = [];

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
    /// 根据已解析的 SchemaMetas 生成 TS 模型文件集合。
    /// </summary>
    public List<GenFileInfo> GenerateModelFiles()
    {
        TsModelFiles.Clear();
        EnumModels.Clear();
        if (SchemaMetas.Count == 0) ParseSchemas();
        int enumCount = 0;
        var tags = ApiTags!.Where(t => t.Name != null);
        foreach (var meta in SchemaMetas)
        {
            string content = TsFormatter.GenerateModel(meta);
            string dir = "models/" + OpenApiHelper.GetNamespaceFirstPart(meta.Namespace).ToHyphen();
            string fileName = meta.Name.ToHyphen() + $".model.ts";
            var file = new GenFileInfo(fileName, content)
            {
                DirName = dir,
                Content = content,
                ModelName = meta.Name,
            };
            TsModelFiles.Add(file);
            if (meta.IsEnum == true)
            {
                EnumModels.Add(meta.Name);
                enumCount++;
            }
        }
        return TsModelFiles;
    }

    /// <summary>
    /// 解析所有 Paths 下的 Operations，生成标准 RequestServiceFunction 集合。
    /// </summary>
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
                var respSchema = operation.Value?.Responses?.FirstOrDefault().Value?.Content?.FirstOrDefault().Value?.Schema;
                function.RequestRefType = OpenApiHelper.GetRootRef(reqSchema);
                function.ResponseRefType = OpenApiHelper.GetRootRef(respSchema);
                function.RequestType = GetTsType(reqSchema);
                function.ResponseType = GetTsType(respSchema);
                function.Params = operation.Value?.Parameters?.Select(p =>
                {
                    string? location = p.In?.GetDisplayName();
                    bool? inpath = location?.ToLower()?.Equals("path");
                    string type = GetTsType(p.Schema);
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
        if (TsModelFiles.Count == 0) GenerateModelFiles();
        if (RequestFunctions.Count == 0) ParseOperations();
        return InternalBuildServices(tags, docName, RequestFunctions);
    }

    /// <summary>
    /// 使用标准 C# 映射与 TypeScript 格式化器生成前端类型
    /// </summary>
    protected static string GetTsType(IOpenApiSchema? schema)
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
        return TsFormatter.FormatType(csharpType, isEnum, isList, isNullable);
    }

    /// <summary>
    /// 获取要导入的依赖
    /// </summary>
    protected List<TypeMeta> GetRefTypes(List<RequestServiceFunction> functions)
    {
        var metaMap = SchemaMetas
            .Where(m => !string.IsNullOrWhiteSpace(m.FullName))
            .GroupBy(m => m.FullName)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        HashSet<string> wanted = [];

        foreach (var f in functions)
        {
            AddIfMatch(f.RequestRefType);
            AddIfMatch(f.ResponseRefType);

            if (f.Params is not null)
            {
                foreach (var p in f.Params)
                {
                    AddIfMatch(p.RefType);
                }
            }

            // 如果响应为泛型，提取其泛型参数的 FullName
            if (!string.IsNullOrWhiteSpace(f.ResponseRefType) && f.ResponseRefType.Contains('`'))
            {
                foreach (var argFullName in ExtractGenericArgumentFullNames(f.ResponseRefType))
                {
                    AddIfMatch(argFullName);
                }
            }
        }

        var result = wanted
            .Where(metaMap.ContainsKey)
            .Select(k => metaMap[k])
            .GroupBy(m => m.FullName)
            .Select(g => g.First())
            .ToList();
        return result;

        void AddIfMatch(string? fullName)
        {
            if (string.IsNullOrWhiteSpace(fullName)) return;
            // 兼容转义的反引号 \u0060 情况
            fullName = fullName.Replace("\u0060", "`");
            if (metaMap.ContainsKey(fullName)) wanted.Add(fullName);
        }

        IEnumerable<string> ExtractGenericArgumentFullNames(string genericFullName)
        {
            int start = genericFullName.IndexOf("[[");
            int end = genericFullName.LastIndexOf("]]");
            if (start < 0 || end <= start) yield break;
            var inner = genericFullName.Substring(start + 2, end - start - 2);
            var args = inner.Split(new[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var arg in args)
            {
                var argType = arg.Split(',')[0].Trim();
                yield return argType;
            }
        }
    }

    protected string InsertImportModel(TypeMeta typeMeta)
    {
        var dirPath = OpenApiHelper.GetNamespaceFirstPart(typeMeta.Namespace).ToHyphen();
        return $"import {{ {typeMeta.Name} }} from '../models/{dirPath}/{typeMeta.Name.ToHyphen()}.model';{Environment.NewLine}";
    }

    protected FunctionBuildResult BuildFunctionCommon(RequestServiceFunction function, bool addExtOptions)
    {
        string name = function.Name;
        List<FunctionParams>? @params = function.Params;
        string requestType = OpenApiHelper.FormatSchemaKey(function.RequestType);
        string responseType = OpenApiHelper.FormatSchemaKey(function.ResponseType);
        string path = function.Path;

        name = name.Replace(function.Tag + "_", "");
        name = name.ToCamelCase();

        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (@params?.Count > 0)
        {
            paramsString = string.Join(
                ", ",
                @params
                    .OrderByDescending(p => p.IsRequired)
                    .Select(p =>
                        p.IsRequired ? p.Name + ": " + OpenApiHelper.FormatSchemaKey(p.Type)
                        : p.Name + ": " + OpenApiHelper.FormatSchemaKey(p.Type) + " | null"
                    )
                    .ToArray()
            );
            @params.ForEach(p =>
            {
                paramsComments += $" * @param {p.Name} {p.Description ?? OpenApiHelper.FormatSchemaKey(p.Type)}\n";
            });
        }
        if (!string.IsNullOrEmpty(requestType))
        {
            if (@params?.Count > 0)
            {
                paramsString += $", data: {requestType}";
            }
            else
            {
                paramsString = $"data: {requestType}";
            }

            dataString = ", data";
            paramsComments += $" * @param data {requestType}\n";
        }
        if (addExtOptions)
        {
            if (!string.IsNullOrWhiteSpace(paramsComments))
            {
                paramsString += ", ";
            }
            paramsString += "extOptions?: ExtOptions";
        }
        // 泛型占位替换：请求类型、响应类型、参数列表
        paramsString = ReplaceGenericPlaceholders(paramsString, function);
        requestType = ReplaceGenericPlaceholders(requestType, function);
        responseType = ReplaceGenericPlaceholders(responseType, function);
        string comments =
            $"/**\n * {function.Description ?? name}\n{paramsComments} */";

        List<string?>? paths = @params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            path = path.Replace(origin, "$" + origin);
        });
        List<string?>? reqParams = @params
            ?.Where(p => !p.InPath && p.Type != "FormData")
            .Select(p => p.Name)
            ?.ToList();
        if (reqParams != null)
        {
            string queryParams = string.Join(
                "&",
                reqParams.Select(p => $"{p}=${{{p} ?? ''}}").ToArray()
            );
            if (!string.IsNullOrEmpty(queryParams))
            {
                path += "?" + queryParams;
            }
        }
        FunctionParams? file = @params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = ", " + file.Name;
        }

        if (addExtOptions)
        {
            if (string.IsNullOrEmpty(dataString))
            {
                dataString = ", null, extOptions";
            }
            else
            {
                dataString += ", extOptions";
            }
        }

        return new FunctionBuildResult(name, paramsString, comments, dataString, path) { ResponseType = responseType };
    }

    private string ReplaceGenericPlaceholders(string text, RequestServiceFunction function)
    {
        if (string.IsNullOrWhiteSpace(text)) return text;
        // 支持基于 RequestRefType/ResponseRefType 的泛型替换, 如 PageList<T> -> PageList<EntityDto>
        var replacements = new Dictionary<string, string>();
        ExtractGenericConcrete(function.RequestRefType, function.RequestType);
        ExtractGenericConcrete(function.ResponseRefType, function.ResponseType);
        foreach (var kv in replacements)
        {
            text = text.Replace(kv.Key, kv.Value);
        }
        return text;

        void ExtractGenericConcrete(string? refType, string? tsType)
        {
            if (string.IsNullOrWhiteSpace(refType) || string.IsNullOrWhiteSpace(tsType)) return;
            // 只处理形如 TypeName<Something>
            int lt = tsType.IndexOf('<');
            int gt = tsType.LastIndexOf('>');
            if (lt > 0 && gt > lt)
            {
                var inner = tsType.Substring(lt + 1, gt - lt - 1);
                // 如果 inner 看起来是占位 (T/T1/T2) 则用 refType 的泛型参数解析
                if (inner.All(c => c == 'T' || char.IsDigit(c) || c == ','))
                {
                    // 从 refType 提取真实参数: PageList`1[[Namespace.EntityDto,...]]
                    var realTypes = OpenApiHelper.ExtractAllTypeNames(refType).Skip(1).Select(OpenApiHelper.FormatSchemaKey).ToList();
                    if (realTypes.Count == 1)
                    {
                        replacements[inner] = realTypes[0];
                    }
                    else if (realTypes.Count > 1)
                    {
                        replacements[inner] = string.Join(",", realTypes);
                    }
                }
            }
        }
    }
}

public record FunctionBuildResult(string Name, string ParamsString, string Comments, string DataString, string Path)
{
    public string ResponseType { get; set; } = string.Empty;
}