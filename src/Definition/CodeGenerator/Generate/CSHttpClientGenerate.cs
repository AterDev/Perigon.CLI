using System.Data;

namespace CodeGenerator.Generate;

/// <summary>
/// c#请求客户端生成
/// </summary>
public class CSHttpClientGenerate(OpenApiDocument openApi) : GenerateBase
{
    /// <summary>
    ///
    /// </summary>
    protected OpenApiPaths PathsPairs { get; } = openApi.Paths;
    protected ISet<OpenApiTag>? ApiTags { get; } = openApi.Tags;
    public IDictionary<string, IOpenApiSchema>? Schemas { get; set; } = openApi.Components?.Schemas;
    public OpenApiDocument OpenApi { get; set; } = openApi;

    public static string GetBaseService(string namespaceName)
    {
        string content = GetTplContent("RequestService.CsharpeBaseService.tpl");
        content = content.Replace("#@Namespace#", namespaceName);
        return content;
    }

    /// <summary>
    /// 生成客户端类
    /// </summary>
    /// <returns></returns>
    public static string GetClient(List<GenFileInfo> infos, string namespaceName, string className)
    {
        string tplContent = GetTplContent("RequestService.CsharpClient.tpl");
        tplContent = tplContent
            .Replace("${Namespace}", namespaceName)
            .Replace("#@ClassName#", className);

        string propsString = "";
        string initPropsString = "";

        infos.ForEach(info =>
        {
            propsString +=
                @$"    public {info.ModelName}Service {info.ModelName} {{ get; init; }}"
                + Environment.NewLine;
            initPropsString +=
                $"        {info.ModelName} = new {info.ModelName}Service(http);"
                + Environment.NewLine;
        });

        tplContent = tplContent
            .Replace("//[@Properties]", propsString)
            .Replace("//[@InitProperties]", initPropsString);

        return tplContent;
    }

    public static string GetGlobalUsing(string name)
    {
        string content = GetTplContent("RequestService.GlobalUsings.tpl");
        content = content + Environment.NewLine + $"global using {name}.Models;";
        content = content + Environment.NewLine + $"global using {name}.Services;";
        return content;
    }

    /// <summary>
    /// 项目文件
    /// </summary>
    /// <returns></returns>
    public static string GetCsprojContent(string dotnetVersion)
    {
        string content = $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <TargetFramework>{dotnetVersion}</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="Microsoft.Extensions.Http"/>
                <PackageReference Include="Microsoft.Extensions.Http.Polly"/>
                <PackageReference Include="Microsoft.Extensions.DependencyInjection"/>
              </ItemGroup>
            </Project>
            """;
        return content;
    }

    /// <summary>
    /// 扩展类
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <param name="services"></param>
    /// <returns></returns>
    public static string GetExtensionContent(string namespaceName, List<string> services)
    {
        string tplContent = GetTplContent("RequestService.Extension.tpl");
        tplContent = tplContent.Replace("#@Namespace#", namespaceName);

        string serviceContent = "";
        services.ForEach(service =>
        {
            serviceContent += $"        services.AddSingleton<{service}>();" + Environment.NewLine;
        });
        tplContent = tplContent.Replace("#@AddServices#", serviceContent);
        return tplContent;
    }

    /// <summary>
    /// 请求服务
    /// </summary>
    /// <param name="namespaceName"></param>
    /// <returns></returns>
    public List<GenFileInfo> GetServices(string namespaceName)
    {
        List<GenFileInfo> files = [];
        List<RequestServiceFunction> functions = GetAllRequestFunctions();

        // 先以tag分组
        List<IGrouping<string?, RequestServiceFunction>> funcGroups = functions
            .GroupBy(f => f.Tag)
            .ToList();
        foreach (IGrouping<string?, RequestServiceFunction>? group in funcGroups)
        {
            // 查询该标签包含的所有方法
            List<RequestServiceFunction> tagFunctions = [.. group];
            OpenApiTag? currentTag = ApiTags?.Where(t => t.Name == group.Key).FirstOrDefault();
            currentTag ??= new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description?.Replace("\r\n", ","),
                Name = currentTag.Name!,
                Functions = tagFunctions,
            };

            string content = ToRequestService(serviceFile, namespaceName);

            string fileName = currentTag.Name + "RestService.cs";
            GenFileInfo file = new(fileName, content) { ModelName = currentTag.Name };
            files.Add(file);
        }
        return files;
    }

    /// <summary>
    /// 获取模型内容
    /// </summary>
    /// <param name="modelName"></param>
    /// <returns></returns>
    public List<GenFileInfo> GetModelFiles(string nspName)
    {
        CsharpModelGenerate csGen = new(OpenApi);
        List<GenFileInfo> files = [];
        if (Schemas == null)
            return files;
        foreach (var item in Schemas)
        {
            files.Add(csGen.GenerateModelFile(item.Key, item.Value, nspName));
        }
        return files;
    }

    public static string ToRequestService(RequestServiceFile serviceFile, string namespaceName)
    {
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string functionstr = "";
        List<string> refTypes = [];
        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(ToRequestFunction).ToArray());
        }
        string result = $$"""
            using {{namespaceName}}.Models;
            namespace {{namespaceName}}.Services;
            /// <summary>
            /// {{serviceFile.Description}}
            /// </summary>
            public class {{serviceFile.Name}}RestService(IHttpClientFactory httpClientFactory) : BaseService(httpClientFactory)
            {
            {{functionstr}}
            }
            """;
        return result;
    }

    public static string ToRequestFunction(RequestServiceFunction function)
    {
        function.ResponseType = string.IsNullOrWhiteSpace(function.ResponseType)
            ? "object"
            : function.ResponseType;

        // 函数名处理，去除tag前缀，然后格式化
        function.Name = function.Name.Replace(function.Tag + "_", "");
        function.Name = function.Name.ToCamelCase();
        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";

        if (function.Params?.Count > 0)
        {
            paramsString = string.Join(
                ", ",
                function
                    .Params.OrderByDescending(p => p.IsRequired)
                    .Select(p => p.IsRequired ? p.Type + " " + p.Name : p.Type + "? " + p.Name)
                    .ToArray()
            );
            function.Params.ForEach(p =>
            {
                //<param name="dto"></param>
                paramsComments +=
                    $"    /// <param name=\"{p.Name}\">{p.Description ?? p.Type} </param>\n";
            });
        }
        if (!string.IsNullOrEmpty(function.RequestType))
        {
            string requestType = function.RequestType == "IFile" ? "Stream" : function.RequestType;
            if (function.Params?.Count > 0)
            {
                paramsString += $", {requestType} data";
            }
            else
            {
                paramsString = $"{requestType} data";
            }

            dataString = ", data";
            paramsComments += $"    /// <param name=\"data\">{requestType}</param>\n";
        }
        // 注释生成
        string comments = $"""
                /// <summary>
                /// {function.Description ?? function.Name}
                /// </summary>
            {paramsComments}    /// <returns></returns>
            """;

        // 构造请求url
        List<string?>? paths = function.Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = function
            .Params?.Where(p => !p.InPath && p.Type != "IForm")
            .Select(p => p.Name)
            ?.ToList();

        if (reqParams != null)
        {
            string queryParams = "";
            queryParams = string.Join(
                "&",
                reqParams
                    .Select(p =>
                    {
                        return $"{p}={{{p}}}";
                    })
                    .ToArray()
            );
            if (!string.IsNullOrEmpty(queryParams))
            {
                function.Path += "?" + queryParams;
            }
        }
        FunctionParams? file = function
            .Params?.Where(p => p.Type!.Equals("FormData"))
            .FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        string returnType = function.ResponseType == "IFile" ? "Stream" : function.ResponseType;

        string method =
            function.ResponseType == "IFile"
                ? $"DownloadFileAsync(url{dataString})"
                : $"{function.Method}JsonAsync<{function.ResponseType}?>(url{dataString})";

        method =
            function.RequestType == "IFile"
                ? $"UploadFileAsync<{function.ResponseType}?>(url, new StreamContent(data))"
                : method;
        string res = $$"""
            {{comments}}
                public async Task<{{returnType}}?> {{function.Name.ToPascalCase()}}Async({{paramsString}}) {
                    var url = $"{{function.Path}}";
                    return await {{method}};
                }

            """;
        return res;
    }

    /// <summary>
    /// 获取方法信息
    /// </summary>
    /// <returns></returns>
    public List<RequestServiceFunction> GetAllRequestFunctions()
    {
        List<RequestServiceFunction> functions = [];
        // 处理所有方法
        foreach (var path in PathsPairs)
        {
            if (path.Value.Operations == null)
                continue;
            foreach (var operation in path.Value.Operations)
            {
                RequestServiceFunction function = new()
                {
                    Description = operation.Value.Summary,
                    Method = operation.Key.ToString(),
                    Name = operation.Value.OperationId ?? (operation.Key.ToString() + path.Key),
                    Path = path.Key,
                    Tag = operation.Value.Tags?.FirstOrDefault()?.Name,
                };
                var reqSchema = operation.Value.RequestBody?.Content?.Values.FirstOrDefault()?.Schema;
                var respSchema = operation.Value.Responses?.FirstOrDefault().Value?.Content?.FirstOrDefault().Value?.Schema;

                function.RequestRefType = OpenApiHelper.GetRootRef(reqSchema);
                function.ResponseRefType = OpenApiHelper.GetRootRef(respSchema);
                function.RequestType = GetCSharpType(reqSchema);
                function.ResponseType = GetCSharpType(respSchema);
                function.Params = operation
                    .Value.Parameters?.Select(p =>
                    {
                        string? location = p.In?.GetDisplayName();
                        bool? inpath = location?.ToLower()?.Equals("path");
                        string type = GetCSharpType(p.Schema);
                        return new FunctionParams
                        {
                            Description = p.Description,
                            Name = p.Name,
                            InPath = inpath ?? false,
                            IsRequired = p.Required,
                            Type = type,
                        };
                    })
                    .ToList();

                functions.Add(function);
            }
        }
        return functions;
    }

    // 统一 schema -> C# 类型解析 (参考 OpenApiHelper.MapToCSharpType 结果 + 枚举/数组/可空处理)
    private static string GetCSharpType(IOpenApiSchema? schema)
    {
        if (schema == null) return "object";

        bool isEnum = (schema.Enum?.Count ?? 0) > 0 || schema.Extensions?.ContainsKey("x-enumData") == true;
        bool isList = schema.Type == JsonSchemaType.Array;
        bool isNullable = schema.Type.HasValue && schema.Type.Value.HasFlag(JsonSchemaType.Null);

        string baseType = OpenApiHelper.MapToCSharpType(schema);
        if (schema is OpenApiSchemaReference r && r.Reference.Id is not null)
        {
            baseType = OpenApiHelper.FormatSchemaKey(r.Reference.Id);
        }

        // 枚举直接用名称
        if (isEnum && schema is OpenApiSchemaReference er && er.Reference.Id is not null)
        {
            baseType = OpenApiHelper.FormatSchemaKey(er.Reference.Id);
        }

        // 列表类型已由 MapToCSharpType 生成 List<...>，只处理可空情况（List 不加 ?）
        if (isNullable && !isList && !baseType.EndsWith("?"))
        {
            // 基本值类型 / 引用类型均加 ? 让生成的 dto 参数可空
            baseType += "?";
        }
        return baseType;
    }
}
