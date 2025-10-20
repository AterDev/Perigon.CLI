using System.ComponentModel;

namespace CodeGenerator.Generate;

/// <summary>
/// 请求生成
/// </summary>
public class RequestGenerate(OpenApiDocument openApi) : GenerateBase
{
    protected OpenApiPaths PathsPairs { get; } = openApi.Paths;
    protected ISet<OpenApiTag>? ApiTags { get; } = openApi.Tags;
    public IDictionary<string, IOpenApiSchema>? Schemas { get; set; } = openApi.Components?.Schemas;
    public OpenApiDocument OpenApi { get; set; } = openApi;

    public RequestClientType LibType { get; set; } = RequestClientType.NgHttp;
    public string? Server { get; set; } = openApi.Servers?.FirstOrDefault()?.Url;

    public List<GenFileInfo> TsModelFiles { get; set; } = [];

    /// <summary>
    /// 枚举类型
    /// </summary>
    public List<string> EnumModels { get; set; } = [];

    public static string GetBaseService(RequestClientType libType)
    {
        try
        {
            switch (libType)
            {
                case RequestClientType.NgHttp:
                    return GetTplContent("angular.base.service.tpl");
                case RequestClientType.Axios:
                    return GetTplContent("RequestService.axios.service.tpl");
                default:
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                "request base service:" + ex.Message + ex.StackTrace + ex.InnerException
            );
            return default!;
        }
        return string.Empty;
    }

    /// <summary>
    /// 获取所有请求接口解析的函数结构
    /// </summary>
    /// <returns></returns>
    public List<RequestServiceFunction> GetAllRequestFunctions()
    {
        List<RequestServiceFunction> functions = [];
        // 处理所有方法
        foreach (var path in PathsPairs)
        {
            if (path.Value.Operations == null || path.Value.Operations.Count == 0)
            {
                continue;
            }
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
                    function.Name = operation.Key + "_" + path.Key.Split("/").LastOrDefault();
                }
                (function.RequestType, function.RequestRefType) = OpenApiHelper.ParseParamTSType(
                    operation.Value?.RequestBody?.Content?.Values.FirstOrDefault()?.Schema
                );
                (function.ResponseType, function.ResponseRefType) = OpenApiHelper.ParseParamTSType(
                    operation
                        .Value?.Responses?.FirstOrDefault()
                        .Value?.Content?.FirstOrDefault()
                        .Value?.Schema
                );
                function.Params = operation
                    .Value?.Parameters?.Select(p =>
                    {
                        string? location = p.In?.GetDisplayName();
                        bool? inpath = location?.ToLower()?.Equals("path");
                        (string type, string? _) = OpenApiHelper.ParseParamTSType(p.Schema);
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

    /// <summary>
    /// 根据tag生成多个请求服务文件
    /// </summary>
    /// <param name="tags"></param>
    /// <returns></returns>
    public List<GenFileInfo> GetServices(ISet<OpenApiTag> tags, string docName)
    {
        if (TsModelFiles.Count == 0)
        {
            GetTSInterfaces();
        }
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
            OpenApiTag? currentTag = tags.Where(t => t.Name == group.Key).FirstOrDefault();
            currentTag ??= new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions,
            };

            string content = LibType switch
            {
                RequestClientType.NgHttp => ToNgRequestBaseService(serviceFile),
                RequestClientType.Axios => ToAxiosRequestService(serviceFile),
                _ => "",
            };

            string path = string.Empty;
            switch (LibType)
            {
                // 同时生成基类和继承类，继承类可自定义
                case RequestClientType.NgHttp:
                    {
                        string baseFileName = currentTag.Name?.ToHyphen() + "-base.service.ts";

                        GenFileInfo file = new(baseFileName, content)
                        {
                            FullName = path,
                            IsCover = true,
                        };
                        files.Add(file);

                        string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
                        content = ToNgRequestService(serviceFile);
                        file = new(fileName, content) { FullName = path, IsCover = false };
                        files.Add(file);
                        break;
                    }
                case RequestClientType.Axios:
                    {
                        string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
                        GenFileInfo file = new(fileName, content) { FullName = path };
                        files.Add(file);
                        break;
                    }
                default:
                    break;
            }
        }
        // api client
        var serviceKeys = funcGroups.Where(g => g.Key != null).Select(g => g.Key).ToList();
        var clientContent = LibType switch
        {
            RequestClientType.NgHttp => ToNgClient(docName, serviceKeys),
            RequestClientType.Axios => "",
            _ => "",
        };
        if (!string.IsNullOrWhiteSpace(clientContent))
        {
            var clientFile = new GenFileInfo($"{docName}-client.ts", clientContent)
            {
                FullName = string.Empty,
                IsCover = true,
                FileType = GenFileType.Global,
            };
            files.Add(clientFile);
        }
        return files;
    }

    /// <summary>
    /// ts interface files
    /// </summary>
    /// <returns></returns>
    public List<GenFileInfo> GetTSInterfaces()
    {
        if (Schemas == null)
            return [];
        TSModelGenerate tsGen = new(OpenApi);
        List<GenFileInfo> files = [];
        foreach (KeyValuePair<string, IOpenApiSchema> item in Schemas)
        {
            var file = tsGen.GenerateInterfaceFile(item.Key, item.Value);
            files.Add(file);
            TsModelFiles.Add(file);
            if (file.DirName == "enum")
            {
                EnumModels.Add(file.ModelName!);
            }
        }
        return files;
    }

    public static string GetEnumPipeContent(
        IDictionary<string, IOpenApiSchema> schemas,
        bool isNgModule = false
    )
    {
        string tplContent = TplContent.EnumPipeTpl(isNgModule);
        string codeBlocks = "";
        foreach (KeyValuePair<string, IOpenApiSchema> item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                codeBlocks += ToEnumSwitchString(OpenApiHelper.FormatSchemaKey(item.Key), item.Value);
            }
        }
        var genContext = new RazorGenContext();
        var model = new CommonViewModel { Content = codeBlocks };
        return genContext.GenCode(tplContent, model);
    }

    /// <summary>
    /// enum function
    /// </summary>
    /// <returns></returns>
    public static string GetEnumFunctionContent(IDictionary<string, IOpenApiSchema> schemas)
    {
        string codeBlocks = "";
        foreach (KeyValuePair<string, IOpenApiSchema> item in schemas)
        {
            if (item.Value.Enum?.Count > 0)
            {
                codeBlocks += ToEnumSwitchString(item.Key, item.Value);
            }
        }
        string? res = $$"""
            export default function enumToString(value: number, type: string): string {
              let result = "";
              switch (type) {
            {{codeBlocks}}
              default:
                break;
              }
              return result;
            }
            """;
        return res;
    }

    public static string ToEnumSwitchString(string enumType, IOpenApiSchema schema)
    {
        var enumProps = OpenApiHelper.GetEnumProperties(schema);

        StringBuilder sb = new();
        var whiteSpace = new string(' ', 12);

        if (enumProps == null || enumProps.Count == 0)
        {
            return "";
        }
        foreach (var prop in enumProps)
        {
            string caseString = string.Format(
                "{0}case {1}: result = '{2}'; break;",
                whiteSpace,
                prop.DefaultValue,
                prop.CommentSummary
            );
            sb.AppendLine(caseString);
        }
        sb.Append($"{whiteSpace}default: result = '默认'; break;");
        string? caseStrings = sb.ToString();
        return $$"""
                  case '{{enumType}}':
                    {
                      switch (value) {
            {{caseStrings}}
                      }
                    }
                    break;

            """;
    }

    public string ToAxiosRequestService(RequestServiceFile serviceFile)
    {
        string tplContent = GetTplContent("RequestService.service.ts");
        string functionString = "";
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        // import引用的models
        string importModels = "";
        if (functions != null)
        {
            functionString = string.Join("\n", functions.Select(ToAxiosFunction).ToArray());
            List<string> refTypes = GetRefTyeps(functions);
            refTypes.ForEach(t =>
            {
                importModels = InsertImportModel(serviceFile, t, importModels);
            });
        }
        tplContent = tplContent
            .Replace("//[@Import]", importModels)
            .Replace("//[@ServiceName]", serviceFile.Name)
            .Replace("//[@Functions]", functionString);
        return tplContent;
    }

    /// <summary>
    /// 生成angular请求服务基类
    /// </summary>
    /// <param name="serviceFile"></param>
    /// <returns></returns>
    public string ToNgRequestBaseService(RequestServiceFile serviceFile)
    {
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string functionstr = "";
        // import引用的models
        string importModels = "";
        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(ToNgRequestFunction).ToArray());
            var refTypes = GetRefTyeps(functions);
            refTypes = refTypes.GroupBy(t => t).Select(g => g.FirstOrDefault()!).ToList();

            refTypes.ForEach(t =>
            {
                importModels = InsertImportModel(serviceFile, t, importModels);
            });
        }
        string result =
            $@"import {{ Injectable }} from '@angular/core';
import {{ BaseService }} from './base.service';
import {{ Observable }} from 'rxjs';
{importModels}
/**
 * {serviceFile.Description}
 */
export class {serviceFile.Name}BaseService extends BaseService {{
{functionstr}
}}
";
        return result;
    }

    /// <summary>
    /// 生成ng 请求服务继承类, 用来自定义
    /// </summary>
    /// <param name="serviceFile"></param>
    /// <returns></returns>
    public static string ToNgRequestService(RequestServiceFile serviceFile)
    {
        string result = $$"""
import { Injectable } from '@angular/core';
import { {{serviceFile.Name}}BaseService } from './{{serviceFile.Name.ToHyphen()}}-base.service';

/**
 * {{serviceFile.Description}}
 */
@Injectable({providedIn: 'root' })
export class {{serviceFile.Name}}Service extends {{serviceFile.Name}}BaseService {
}
""";
        return result;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="docName"></param>
    /// <param name="serviceNames"></param>
    /// <returns></returns>
    private string ToNgClient(string docName, List<string> serviceNames)
    {
        string imports = "";
        string injects = "";
        serviceNames.ForEach(s =>
        {
            string className = s + "Service";
            imports +=
                $"import {{ {className} }} from './{s.ToHyphen()}.service';{Environment.NewLine}";
            injects += $"  public {s.ToCamelCase()} = inject({className});{Environment.NewLine}";
        });

        return $$"""
            import { inject, Injectable } from "@angular/core";
            {{imports}}

            @Injectable({
              providedIn: 'root'
            })
            export class AdminClient {
            {{injects}}
            }
            """;
    }

    /// <summary>
    /// axios函数格式
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    private string ToAxiosFunction(RequestServiceFunction function)
    {
        var result = BuildFunctionCommon(function, true);

        string responseType = string.IsNullOrWhiteSpace(function.ResponseType)
            ? "any"
            : function.ResponseType!;
        responseType = OpenApiHelper.FormatSchemaKey(responseType);

        string functionString =
            @$"{result.Comments}
  {result.Name}({result.ParamsString}): Promise<{responseType}> {{
    const _url = `{result.Path}`;
    return this.request<{responseType}>('{function.Method.ToLower()}', _url{result.DataString});
  }}
";
        return functionString;
    }

    private string ToNgRequestFunction(RequestServiceFunction function)
    {
        var result = BuildFunctionCommon(function, false);

        string responseType = string.IsNullOrWhiteSpace(function.ResponseType)
            ? "any"
            : function.ResponseType!;

        responseType = OpenApiHelper.FormatSchemaKey(responseType);
        string method = "request";
        string generics = $"<{responseType}>";
        if (responseType.Equals("FormData"))
        {
            responseType = "Blob";
            method = "downloadFile";
            generics = "";
        }

        string functionString =
            @$"{result.Comments}
  {result.Name}({result.ParamsString}): Observable<{responseType}> {{
    const _url = `{result.Path}`;
    return this.{method}{generics}('{function.Method.ToLower()}', _url{result.DataString});
  }}
";
        return functionString;
    }

    /// <summary>
    /// 构建函数公共部分
    /// </summary>
    /// <param name="function"></param>
    /// <param name="addExtOptions"></param>
    /// <returns></returns>
    private FunctionBuildResult BuildFunctionCommon(RequestServiceFunction function, bool addExtOptions)
    {
        string name = function.Name;
        List<FunctionParams>? @params = function.Params;
        string requestType = OpenApiHelper.FormatSchemaKey(function.RequestType);
        string path = function.Path;

        // 函数名处理，去除tag前缀，然后格式化
        name = name.Replace(function.Tag + "_", "");
        name = name.ToCamelCase();

        // 处理参数
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
                paramsComments += $"   * @param {p.Name} {p.Description ?? OpenApiHelper.FormatSchemaKey(p.Type)}\n";
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
            paramsComments += $"   * @param data {requestType}\n";
        }
        // 添加extOptions
        if (addExtOptions)
        {
            if (!string.IsNullOrWhiteSpace(paramsComments))
            {
                paramsString += ", ";
            }
            paramsString += "extOptions?: ExtOptions";
        }
        // 注释生成
        string comments =
            $@"  /**
   * {function.Description ?? name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = @params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            path = path.Replace(origin, "$" + origin);
        });
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = @params
            ?.Where(p => !p.InPath && p.Type != "FormData")
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
                        return $"{p}=${{{p} ?? ''}}";
                    })
                    .ToArray()
            );
            if (!string.IsNullOrEmpty(queryParams))
            {
                path += "?" + queryParams;
            }
        }
        // 上传文件时的名称
        FunctionParams? file = @params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        // 默认添加ext
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

        return new FunctionBuildResult(name, paramsString, comments, dataString, path);
    }

    /// <summary>
    /// 模板的引用
    /// </summary>
    /// <param name="serviceFile"></param>
    /// <param name="t"></param>
    /// <param name="importModels"></param>
    /// <returns></returns>
    private string InsertImportModel(RequestServiceFile serviceFile, string t, string importModels)
    {

        var formatType = OpenApiHelper.FormatSchemaKey(t);
        if (EnumModels.Contains(t))
        {
            importModels +=
                $"import {{ {formatType} }} from './enum/{formatType.ToHyphen()}.model';{Environment.NewLine}";
        }
        else
        {
            importModels +=
                $"import {{ {formatType} }} from './models/{formatType.ToHyphen()}.model';{Environment.NewLine}";
        }
        return importModels;
    }

    /// <summary>
    /// 获取要导入的依赖
    /// </summary>
    /// <param name="functions"></param>
    /// <returns></returns>
    protected List<string> GetRefTyeps(List<RequestServiceFunction> functions)
    {
        // 已生成模型的名称集合（包含枚举）
        var modelNameSet = TsModelFiles
            .Where(f => !string.IsNullOrWhiteSpace(f.ModelName))
            .Select(f => f.ModelName!)
            .ToHashSet();

        HashSet<string> refTypes = [];

        foreach (var f in functions)
        {
            if (!string.IsNullOrWhiteSpace(f.RequestRefType) && modelNameSet.Contains(f.RequestRefType, StringComparer.OrdinalIgnoreCase))
                refTypes.Add(f.RequestRefType);

            if (!string.IsNullOrWhiteSpace(f.ResponseRefType) && modelNameSet.Contains(f.ResponseRefType, StringComparer.OrdinalIgnoreCase))
                refTypes.Add(f.ResponseRefType);

            if (f.Params != null)
            {
                foreach (var p in f.Params)
                {
                    if (!string.IsNullOrWhiteSpace(p.Type) && modelNameSet.Contains(p.Type, StringComparer.OrdinalIgnoreCase))
                        refTypes.Add(p.Type);
                }
            }
        }
        return refTypes.ToList();
    }
}

public enum RequestClientType
{
    [Description("angular http")]
    NgHttp,

    [Description("axios")]
    Axios,

    [Description("csharp")]
    Csharp,
}

public record FunctionBuildResult(string Name, string ParamsString, string Comments, string DataString, string Path);
