using CodeGenerator.Generate.LanguageFormatter;
using CodeGenerator.Helper;
using CodeGenerator.Models;

namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// Axios 客户端生成器
/// </summary>
public class AxiosClient(OpenApiDocument openApi) : ClientRequestBase(openApi)
{
    protected override List<GenFileInfo> InternalBuildServices(ISet<OpenApiTag> tags, string docName, List<RequestServiceFunction> functions)
    {
        List<GenFileInfo> files = [];
        var funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (var group in funcGroups)
        {
            var tagFunctions = group.ToList();
            OpenApiTag? currentTag = tags.FirstOrDefault(t => t.Name == group.Key) ?? new OpenApiTag { Name = group.Key, Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions,
            };
            string content = ToAxiosRequestService(serviceFile);
            string path = string.Empty;
            string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
            GenFileInfo file = new(fileName, content) { FullName = path };
            files.Add(file);
        }
        return files;
    }

    private string ToAxiosRequestService(RequestServiceFile serviceFile)
    {
        string tplContent = GenerateBase.GetTplContent("RequestService.service.ts");
        string functionString = "";
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string importModels = "";
        if (functions != null)
        {
            functionString = string.Join(Environment.NewLine, functions.Select(ToAxiosFunction).ToArray());
            List<string> refTypes = GetRefTyeps(functions);
            refTypes.ForEach(t => importModels += InsertImportModel(t));
        }
        // 保持模板与统一缩进: 函数字符串已经由 TsCodeWriter 缩进，直接插入
        tplContent = tplContent
            .Replace("//[@Import]", importModels.TrimEnd())
            .Replace("//[@ServiceName]", serviceFile.Name)
            .Replace("//[@Functions]", functionString.TrimEnd());
        return tplContent;
    }

    private string ToAxiosFunction(RequestServiceFunction function)
    {
        var result = BuildFunctionCommon(function, true);
        string responseType = string.IsNullOrWhiteSpace(result.ResponseType) ? "any" : OpenApiHelper.FormatSchemaKey(result.ResponseType);
        var cw = new Helper.TsCodeWriter();
        foreach (var line in result.Comments.Split('\n')) cw.AppendLine(line);
        cw.AppendLine($"{result.Name}({result.ParamsString}): Promise<{responseType}> {{").Indent();
        cw.AppendLine($"const _url = `{result.Path}`;");
        cw.AppendLine($"return this.request<{responseType}>('{function.Method.ToLower()}', _url{result.DataString});");
        cw.Unindent().AppendLine("}");
        return cw.ToString().TrimEnd();
    }
}