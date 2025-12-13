using CodeGenerator.Generate.ClientRequest;
using CodeGenerator.Generate.LanguageFormatter;
using System.Data;

namespace CodeGenerator.Generate;

/// <summary>
/// c#请求客户端生成
/// </summary>
public class CSHttpClientGenerate(OpenApiDocument openApi) : ClientRequestBase(openApi)
{
    protected override LanguageFormatterBase Formatter { get; } = new CSharpFormatter();

    public static string GetBaseService(string namespaceName)
    {
        string content = GenerateBase.GetTplContent("RequestService.CsharpeBaseService.tpl");
        content = content.Replace("#@Namespace#", namespaceName);
        return content;
    }

    /// <summary>
    /// 生成客户端类
    /// </summary>
    /// <returns></returns>
    public static string GetClient(List<GenFileInfo> infos, string namespaceName, string className)
    {
        string tplContent = GenerateBase.GetTplContent("RequestService.CsharpClient.tpl");
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
        string content = GenerateBase.GetTplContent("RequestService.GlobalUsings.tpl");
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
                <PackageReference Include="Microsoft.Extensions.Http" VersionOverride="10.0.1"/>
                <PackageReference Include="Microsoft.Extensions.Http.Polly" VersionOverride="10.0.1"/>
                <PackageReference Include="Microsoft.Extensions.DependencyInjection" VersionOverride="10.0.1"/>
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
        string tplContent = GenerateBase.GetTplContent("RequestService.Extension.tpl");
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
    /// <param name="projectName"></param>
    /// <returns></returns>
    public List<GenFileInfo> GetServices(string projectName)
    {
        // 使用基类方法生成服务
        if (SchemaMetas.Count == 0) ParseSchemas();
        if (RequestFunctions.Count == 0) ParseOperations();
        if (ModelFiles.Count == 0) GetModelFiles(projectName);

        return InternalBuildServices(ApiTags!, projectName, RequestFunctions);
    }

    protected override List<GenFileInfo> InternalBuildServices(ISet<OpenApiTag> tags, string nspName, List<RequestServiceFunction> functions)
    {
        List<GenFileInfo> files = [];
        // 先以tag分组
        List<IGrouping<string?, RequestServiceFunction>> funcGroups = functions
            .GroupBy(f => f.Tag)
            .ToList();
        foreach (IGrouping<string?, RequestServiceFunction>? group in funcGroups)
        {
            // 查询该标签包含的所有方法
            List<RequestServiceFunction> tagFunctions = [.. group];
            OpenApiTag? currentTag = tags?.Where(t => t.Name == group.Key).FirstOrDefault();
            currentTag ??= new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description?.Replace("\r\n", ","),
                Name = currentTag.Name!,
                Functions = tagFunctions,
            };

            string content = ToRequestService(serviceFile, nspName);

            string fileName = currentTag.Name + "RestService.cs";
            GenFileInfo file = new(fileName, content) { ModelName = currentTag.Name };
            files.Add(file);
        }
        return files;
    }


    public List<GenFileInfo> GetModelFiles(string projectName)
    {
        ModelFiles.Clear();
        EnumModels.Clear();
        if (SchemaMetas.Count == 0) ParseSchemas();
        if (RequestFunctions.Count == 0) ParseOperations();

        foreach (var meta in SchemaMetas)
        {
            string moduleName = OpenApiHelper.GetNamespaceFirstPart(meta.Namespace) ?? string.Empty;
            string content = Formatter.GenerateModel(meta, projectName);
            var schemaKey = meta.Name;
            schemaKey = OpenApiHelper.FormatSchemaKey(schemaKey);
            string fileName = schemaKey.ToPascalCase() + ".cs";

            var file = new GenFileInfo(fileName, content)
            {
                DirName = moduleName,
                Content = content,
                ModelName = meta.Name,
                ModuleName = moduleName,
            };
            ModelFiles.Add(file);
            if (meta.IsEnum == true)
            {
                EnumModels.Add(meta.Name);
            }
        }
        return ModelFiles;
    }

    public override List<GenFileInfo> GenerateModelFiles()
    {
        // C# 需要 nspName，但基类方法签名没有参数。
        // 这里我们可以抛出异常，或者使用默认命名空间，或者重载一个带参数的方法供外部调用。
        // 由于 GetModelFiles(string nspName) 已经存在并被调用，我们可以让它调用这个重载。
        // 但为了满足基类契约，我们需要实现这个无参方法。
        // 我们可以暂存 nspName 或者在构造函数中传入。
        // 鉴于目前的调用方式，GetModelFiles(nspName) 是入口。
        // 我们可以让 GetModelFiles() 返回空或者抛出异常，因为它不应该被直接调用。
        // 或者，我们可以修改基类 GetModelFiles 接受可选参数。
        return [];
    }

    public string ToRequestService(RequestServiceFile serviceFile, string namespaceName)
    {
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string functionstr = "";
        List<string> refTypes = functions?.Select(f => f.ResponseRefType)
            .Concat(functions.Select(f => f.RequestRefType))
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Distinct()
            .Select(r => OpenApiHelper.GetNamespaceFirstPart(r))
            .Distinct()
            .ToList() ?? [];


        var usingStrings = refTypes.Select(r => $"using {namespaceName}.Models.{r};")
            .ToList();

        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(ToRequestFunction).ToArray());
        }

        string result = $$"""
            {{string.Join(Environment.NewLine, usingStrings)}}
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
        function.Name = function
            .Name
            .Replace(function.Tag + "_", "");
        function.Name = function
            .Name
            .ToCamelCase();
        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";

        if (function.Params?.Count > 0)
        {
            var orderedParams = function.Params.OrderByDescending(p => p.IsRequired).ToList();
            foreach (var p in orderedParams)
            {
                if (!string.IsNullOrEmpty(paramsString))
                {
                    paramsString += ", ";
                }
                var typeName = ReplaceGenericPlaceholders(OpenApiHelper.FormatSchemaKey(p.Type), function);
                paramsString += p.IsRequired
                    ? typeName + " " + p.Name
                    : typeName + "? " + p.Name;
                paramsComments +=
                    $"    /// <param name=\"{p.Name}\">{p.Description ?? typeName} </param>\n";
            }
        }
        if (!string.IsNullOrEmpty(function.RequestType))
        {
            string requestType = function.RequestType == "IFile"
                ? "Stream"
                : OpenApiHelper.FormatSchemaKey(function.RequestType);

            requestType = ReplaceGenericPlaceholders(requestType, function);
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

        string returnType = function.ResponseType == "IFile"
            ? "Stream"
            : OpenApiHelper.FormatSchemaKey(function.ResponseType);

        returnType = ReplaceGenericPlaceholders(returnType, function);

        string method =
            function.ResponseType == "IFile"
                ? $"DownloadFileAsync(url{dataString})"
                : $"{function.Method.ToLower().ToUpperFirst()}JsonAsync<{returnType}?>(url{dataString})";

        method =
            function.RequestType == "IFile"
                ? $"UploadFileAsync<{function.ResponseType}?>(url, new StreamContent(data))"
                : method;
        string res = $$"""
            {{comments}}
                public async Task<{{returnType}}?> {{function.Name.ToPascalCase()}}Async({{paramsString}}) 
                {
                    var url = $"{{function.Path}}";
                    return await {{method}};
                }

            """;
        return res;
    }
}
