namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// Angular Http 客户端生成器
/// </summary>
public class AngularClient(OpenApiDocument openApi) : ClientRequestBase(openApi)
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
            string content = ToNgRequestBaseService(serviceFile);
            string path = string.Empty;

            string baseFileName = currentTag.Name?.ToHyphen() + "-base.service.ts";
            GenFileInfo baseFile = new(baseFileName, content) { FullName = path, IsCover = true };
            files.Add(baseFile);
            string fileName = currentTag.Name?.ToHyphen() + ".service.ts";
            var implContent = ToNgRequestService(serviceFile);
            GenFileInfo implFile = new(fileName, implContent) { FullName = path, IsCover = false };
            files.Add(implFile);
        }
        var serviceKeys = functions.Where(f => !string.IsNullOrWhiteSpace(f.Tag)).Select(f => f.Tag!).Distinct().ToList();
        var clientContent = ToNgClient(docName, serviceKeys);
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

    private string ToNgRequestBaseService(RequestServiceFile serviceFile)
    {
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string functionstr = "";
        string importModels = "";
        if (functions != null)
        {
            functionstr = string.Join(Environment.NewLine, functions.Select(ToNgRequestFunction).ToArray());
            var refTypes = GetRefTyeps(functions).GroupBy(t => t).Select(g => g.First()).ToList();
            refTypes.ForEach(t => importModels += InsertImportModel(t));
        }
        var cw = new Helper.TsCodeWriter();
        cw.AppendLine("import { Injectable } from '@angular/core';")
            .AppendLine("import { BaseService } from './base.service';")
            .AppendLine("import { Observable } from 'rxjs';");
        if (!string.IsNullOrWhiteSpace(importModels)) cw.AppendLine(importModels.TrimEnd());
        cw.AppendLine("/**")
            .AppendLine($" * {serviceFile.Description}")
            .AppendLine(" */")
            .OpenBlock($"export class {serviceFile.Name}BaseService extends BaseService");
        if (!string.IsNullOrWhiteSpace(functionstr))
        {
            foreach (var line in functionstr.Split(Environment.NewLine))
            {
                cw.AppendLine(line);
            }
        }
        cw.CloseBlock();
        return cw.ToString().TrimEnd();
    }

    private static string ToNgRequestService(RequestServiceFile serviceFile)
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

    private string ToNgClient(string docName, List<string> serviceNames)
    {
        var cw = new Helper.TsCodeWriter();
        cw.AppendLine("import { inject, Injectable } from '@angular/core';");
        foreach (var s in serviceNames)
        {
            string className = s + "Service";
            cw.AppendLine($"import {{ {className} }} from './{s.ToHyphen()}.service';");
        }
        cw.AppendLine().AppendLine("@Injectable({").Indent();
        cw.AppendLine("providedIn: 'root'");
        cw.Unindent().AppendLine("})");
        cw.OpenBlock("export class AdminClient");
        foreach (var s in serviceNames)
        {
            string className = s + "Service";
            cw.AppendLine($"public {s.ToCamelCase()} = inject({className});");
        }
        cw.CloseBlock();
        return cw.ToString();
    }

    private string ToNgRequestFunction(RequestServiceFunction function)
    {
        var result = BuildFunctionCommon(function, false);
        string responseType = string.IsNullOrWhiteSpace(result.ResponseType) ? "any" : OpenApiHelper.FormatSchemaKey(result.ResponseType);
        string method = "request";
        string generics = $"<{responseType}>";
        if (responseType.Equals("FormData"))
        {
            responseType = "Blob";
            method = "downloadFile";
            generics = "";
        }
        var cw = new Helper.TsCodeWriter();
        // 对齐注释块：在方法起始缩进级别输出注释
        if (!string.IsNullOrWhiteSpace(result.Comments))
        {
            var commentLines = result.Comments.Split('\n');
            // 确保首行 /** 末行 */ 与中间 * 对齐
            foreach (var line in commentLines)
            {
                cw.AppendLine(line.TrimEnd());
            }
        }
        cw.OpenBlock($"{result.Name}({result.ParamsString}): Observable<{responseType}>");
        cw.AppendLine($"const _url = `{result.Path}`;");
        cw.AppendLine($"return this.{method}{generics}('{function.Method.ToLower()}', _url{result.DataString});");
        cw.CloseBlock();
        return cw.ToString().TrimEnd();
    }
}