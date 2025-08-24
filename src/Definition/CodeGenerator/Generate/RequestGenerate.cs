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
                    Name = operation.Value?.OperationId ?? operation.Key.ToString() + path.Key,
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
    public List<GenFileInfo> GetServices(ISet<OpenApiTag> tags)
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

            string path = currentTag.Name?.ToHyphen() ?? "";
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
                codeBlocks += ToEnumSwitchString(item.Key, item.Value);
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
        List<string> refTypes = [];
        if (functions != null)
        {
            functionstr = string.Join("\n", functions.Select(ToNgRequestFunction).ToArray());
            string[] baseTypes = ["string", "string[]", "number", "number[]", "boolean", "integer"];
            // 获取请求和响应的类型，以便导入
            List<string?> requestRefs = functions
                .Where(f =>
                    !string.IsNullOrEmpty(f.RequestRefType) && !baseTypes.Contains(f.RequestRefType)
                )
                .Select(f => f.RequestRefType)
                .ToList();
            List<string?> responseRefs = functions
                .Where(f =>
                    !string.IsNullOrEmpty(f.ResponseRefType)
                    && !baseTypes.Contains(f.ResponseRefType)
                )
                .Select(f => f.ResponseRefType)
                .ToList();

            // 参数中的类型
            List<string?> paramsRefs = functions
                .Where(f => f.Params != null)
                .SelectMany(f => f.Params!)
                .Where(p => !baseTypes.Contains(p.Type))
                .Select(p => p.Type)
                .ToList();

            if (requestRefs != null)
            {
                refTypes.AddRange(requestRefs!);
            }

            if (responseRefs != null)
            {
                refTypes.AddRange(responseRefs!);
            }

            if (paramsRefs != null)
            {
                refTypes.AddRange(paramsRefs!);
            }

            refTypes = refTypes.GroupBy(t => t).Select(g => g.FirstOrDefault()!).ToList();

            refTypes.ForEach(t =>
            {
                importModels = InsertImportModel(serviceFile, t, importModels);
            });
        }
        string result =
            $@"import {{ Injectable }} from '@angular/core';
import {{ BaseService }} from '../base.service';
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
    /// 生成ng 请求服务继承类,可自定义
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
  id: string | null = null;
  name: string | null = null;
}
""";
        return result;
    }

    /// <summary>
    /// axios函数格式
    /// </summary>
    /// <param name="function"></param>
    /// <returns></returns>
    protected string ToAxiosFunction(RequestServiceFunction function)
    {
        string Name = function.Name;
        List<FunctionParams>? Params = function.Params;
        string RequestType = function.RequestType;
        string ResponseType = string.IsNullOrWhiteSpace(function.ResponseType)
            ? "any"
            : function.ResponseType!;

        string Path = function.Path;
        if (Server != null)
        {
            if (!Server.StartsWith("http://") && Server.StartsWith("https://"))
            {
                Path = Server + Path;
            }
        }

        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(function.Tag + "_", "");
        Name = Name.ToCamelCase();
        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (Params?.Count > 0)
        {
            paramsString = string.Join(
                ", ",
                Params
                    .OrderByDescending(p => p.IsRequired)
                    .Select(p =>
                        p.IsRequired ? p.Name + ": " + p.Type : p.Name + ": " + p.Type + " | null"
                    )
                    .ToArray()
            );
            Params.ForEach(p =>
            {
                paramsComments += $"   * @param {p.Name} {p.Description ?? p.Type}\n";
            });
        }
        if (!string.IsNullOrEmpty(RequestType))
        {
            if (Params?.Count > 0)
            {
                paramsString += $", data: {RequestType}";
            }
            else
            {
                paramsString = $"data: {RequestType}";
            }

            dataString = ", data";
            paramsComments += $"   * @param data {RequestType}\n";
        }
        // 添加extOptions
        if (!string.IsNullOrWhiteSpace(paramsComments))
        {
            paramsString += ", ";
        }
        paramsString += "extOptions?: ExtOptions";
        // 注释生成
        string comments =
            $@"  /**
   * {function.Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            Path = Path.Replace(origin, "$" + origin);
        });
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = Params
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
                Path += "?" + queryParams;
            }
        }
        // 上传文件时的名称
        FunctionParams? file = Params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        // 默认添加ext
        if (string.IsNullOrEmpty(dataString))
        {
            dataString = ", null, extOptions";
        }
        else
        {
            dataString += ", extOptions";
        }
        string functionString =
            @$"{comments}
  {Name}({paramsString}): Promise<{ResponseType}> {{
    const _url = `{Path}`;
    return this.request<{ResponseType}>('{function.Method.ToLower()}', _url{dataString});
  }}
";
        return functionString;
    }

    public string ToNgRequestFunction(RequestServiceFunction function)
    {
        string Name = function.Name;
        List<FunctionParams>? Params = function.Params;
        string RequestType = function.RequestType;
        string ResponseType = string.IsNullOrWhiteSpace(function.ResponseType)
            ? "any"
            : function.ResponseType!;

        string Path = function.Path;
        if (Server != null)
        {
            Path = Server + Path;
        }

        // 函数名处理，去除tag前缀，然后格式化
        Name = Name.Replace(function.Tag + "_", "");
        Name = Name.ToCamelCase();

        // 处理参数
        string paramsString = "";
        string paramsComments = "";
        string dataString = "";
        if (Params?.Count > 0)
        {
            paramsString = string.Join(
                ", ",
                Params
                    .OrderByDescending(p => p.IsRequired)
                    .Select(p =>
                        p.IsRequired ? p.Name + ": " + p.Type : p.Name + ": " + p.Type + " | null"
                    )
                    .ToArray()
            );
            Params.ForEach(p =>
            {
                paramsComments += $"   * @param {p.Name} {p.Description ?? p.Type}\n";
            });
        }

        if (!string.IsNullOrEmpty(RequestType))
        {
            if (Params?.Count > 0)
            {
                paramsString += $", data: {RequestType}";
            }
            else
            {
                paramsString = $"data: {RequestType}";
            }

            dataString = ", data";
            paramsComments += $"   * @param data {RequestType}\n";
        }
        // 注释生成
        string comments =
            $@"  /**
   * {function.Description ?? Name}
{paramsComments}   */";

        // 构造请求url
        List<string?>? paths = Params?.Where(p => p.InPath).Select(p => p.Name)?.ToList();
        paths?.ForEach(p =>
        {
            string origin = $"{{{p}}}";
            Path = Path.Replace(origin, "$" + origin);
        });
        // 需要拼接的参数,特殊处理文件上传
        List<string?>? reqParams = Params
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
                Path += "?" + queryParams;
            }
        }
        FunctionParams? file = Params?.Where(p => p.Type!.Equals("FormData")).FirstOrDefault();
        if (file != null)
        {
            dataString = $", {file.Name}";
        }

        string method = "request";
        string generics = $"<{ResponseType}>";
        if (ResponseType.Equals("FormData"))
        {
            ResponseType = "Blob";
            method = "downloadFile";
            generics = "";
        }

        string functionString =
            @$"{comments}
  {Name}({paramsString}): Observable<{ResponseType}> {{
    const _url = `{Path}`;
    return this.{method}{generics}('{function.Method.ToLower()}', _url{dataString});
  }}
";
        return functionString;
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
        if (EnumModels.Contains(t))
        {
            importModels +=
                $"import {{ {t} }} from '../enum/models/{t.ToHyphen()}.model';{Environment.NewLine}";
        }
        else
        {
            string? dirName = TsModelFiles
                .Where(f => f.ModelName == t)
                .Select(f => f.DirName)
                .FirstOrDefault();

            if (dirName != serviceFile.Name.ToHyphen())
            {
                importModels +=
                    $"import {{ {t} }} from '../{dirName}/models/{t.ToHyphen()}.model';{Environment.NewLine}";
            }
            else
            {
                importModels +=
                    $"import {{ {t} }} from './models/{t.ToHyphen()}.model';{Environment.NewLine}";
            }
        }
        return importModels;
    }

    /// <summary>
    /// 获取要导入的依赖
    /// </summary>
    /// <param name="functions"></param>
    /// <returns></returns>
    protected static List<string> GetRefTyeps(List<RequestServiceFunction> functions)
    {
        List<string> refTypes = [];

        string[] baseTypes = ["string", "string[]", "number", "number[]", "boolean", "integer"];
        // 获取请求和响应的类型，以便导入
        List<string?> requestRefs = functions
            .Where(f =>
                !string.IsNullOrEmpty(f.RequestRefType) && !baseTypes.Contains(f.RequestRefType)
            )
            .Select(f => f.RequestRefType)
            .ToList();
        List<string?> responseRefs = functions
            .Where(f =>
                !string.IsNullOrEmpty(f.ResponseRefType) && !baseTypes.Contains(f.ResponseRefType)
            )
            .Select(f => f.ResponseRefType)
            .ToList();

        // 参数中的类型
        List<string?> paramsRefs = functions
            .SelectMany(f => f.Params!)
            .Where(p => !baseTypes.Contains(p.Type))
            .Select(p => p.Type)
            .ToList();
        if (requestRefs != null)
        {
            refTypes.AddRange(requestRefs!);
        }

        if (responseRefs != null)
        {
            refTypes.AddRange(responseRefs!);
        }

        if (paramsRefs != null)
        {
            refTypes.AddRange(paramsRefs!);
        }

        refTypes = refTypes.GroupBy(t => t).Select(g => g.FirstOrDefault()!).ToList();
        return refTypes;
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
