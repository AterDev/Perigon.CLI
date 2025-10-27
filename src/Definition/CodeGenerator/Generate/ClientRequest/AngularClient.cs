namespace CodeGenerator.Generate.ClientRequest;

/// <summary>
/// Angular Http 客户端生成器
/// </summary>
public class AngularClient(OpenApiDocument openApi) : ClientRequestBase(openApi)
{
    // the rquest service file path
    private readonly string servicePath = "services";

    protected override List<GenFileInfo> InternalBuildServices(
        ISet<OpenApiTag> tags,
        string docName,
        List<RequestServiceFunction> functions
    )
    {
        List<GenFileInfo> files = [];
        var funcGroups = functions.GroupBy(f => f.Tag).ToList();
        foreach (var group in funcGroups)
        {
            var tagFunctions = group.ToList();
            var currentTag =
                tags.FirstOrDefault(t => t.Name == group.Key)
                ?? new OpenApiTag { Name = group.Key ?? "", Description = group.Key };
            RequestServiceFile serviceFile = new()
            {
                Description = currentTag.Description,
                Name = currentTag.Name!,
                Functions = tagFunctions,
            };

            string content = ToNgRequestService(serviceFile);

            string baseFileName = currentTag.Name?.ToHyphen() + ".service.ts";
            GenFileInfo baseFile = new(baseFileName, content)
            {
                DirName = servicePath,
                IsCover = true,
            };
            files.Add(baseFile);
        }
        var serviceKeys = functions
            .Where(f => !string.IsNullOrWhiteSpace(f.Tag))
            .Select(f => f.Tag!)
            .Distinct()
            .ToList();
        var clientContent = ToNgClient(docName, serviceKeys);
        if (!string.IsNullOrWhiteSpace(clientContent))
        {
            var clientFile = new GenFileInfo($"{docName}-client.ts", clientContent)
            {
                DirName = string.Empty,
                IsCover = true,
                FileType = GenFileType.Global,
            };
            files.Add(clientFile);
        }
        return files;
    }

    private string ToNgRequestService(RequestServiceFile serviceFile)
    {
        List<RequestServiceFunction>? functions = serviceFile.Functions;
        string functionstr = "";
        string importModels = "";
        if (functions != null)
        {
            functionstr = string.Join(
                Environment.NewLine,
                functions.Select(ToNgRequestFunction).ToArray()
            );
            var refMetas = GetRefTypes(functions)
                .GroupBy(m => m.Name)
                .Select(g => g.First())
                .ToList();
            refMetas.ForEach(m => importModels += InsertImportModel(m));
        }
        var cw = new CodeWriter();
        cw.AppendLine("import { BaseService } from '../base.service';")
            .AppendLine("import { Injectable } from '@angular/core';")
            .AppendLine("import { Observable } from 'rxjs';");
        if (!string.IsNullOrWhiteSpace(importModels))
            cw.AppendLine(importModels.TrimEnd());
        cw.AppendLine("/**")
            .AppendLine($" * {serviceFile.Description}")
            .AppendLine(" */")
            .AppendLine("@Injectable({ providedIn: 'root' })")
            .OpenBlock($"export class {serviceFile.Name}Service extends BaseService");
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

    private string ToNgClient(string docName, List<string> serviceNames)
    {
        var cw = new CodeWriter();
        cw.AppendLine("import { inject, Injectable } from '@angular/core';");
        foreach (var s in serviceNames)
        {
            string className = s + "Service";
            cw.AppendLine(
                $"import {{ {className} }} from './{servicePath}/{s.ToHyphen()}.service';"
            );
        }
        cw.AppendLine().AppendLine("@Injectable({").Indent();
        cw.AppendLine("providedIn: 'root'");
        cw.Unindent().AppendLine("})");
        cw.OpenBlock($"export class {docName.ToPascalCase()}Client");
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
        string responseType = string.IsNullOrWhiteSpace(result.ResponseType)
            ? "any"
            : OpenApiHelper.FormatSchemaKey(result.ResponseType);
        string method = "request";
        string generics = $"<{responseType}>";
        if (responseType.Equals("FormData"))
        {
            responseType = "Blob";
            generics = "";
        }
        var cw = new Helper.CodeWriter();
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
        cw.AppendLine(
            $"return this.{method}{generics}('{function.Method.ToLower()}', _url{result.DataString});"
        );
        cw.CloseBlock();
        return cw.ToString().TrimEnd();
    }
}
