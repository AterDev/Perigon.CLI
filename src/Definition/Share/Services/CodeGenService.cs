using System.ComponentModel;
using System.Diagnostics;
using CodeGenerator;
using CodeGenerator.Generate;
using CodeGenerator.Models;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;

namespace Share.Services;
/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService(ILogger<CodeGenService> logger)
{
    private readonly ILogger<CodeGenService> _logger = logger;

    /// <summary>
    /// 生成Dto
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="outputPath">输出项目目录</param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>> GenerateDtosAsync(EntityInfo entityInfo, string outputPath, bool isCover = false)
    {
        _logger.LogInformation("🚀 Generating Dtos...");
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo);
        var dirName = entityInfo.Name + "Dtos";
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, dtoGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };

        var addDtoFile = GenerateDto(entityInfo, DtoType.Add);
        addDtoFile.IsCover = isCover;
        addDtoFile.FullName = Path.Combine(outputPath, addDtoFile.FullName);

        var updateDtoFile = GenerateDto(entityInfo, DtoType.Update);
        updateDtoFile.IsCover = isCover;
        updateDtoFile.FullName = Path.Combine(outputPath, updateDtoFile.FullName);

        var filterDtoFile = GenerateDto(entityInfo, DtoType.Filter);
        filterDtoFile.IsCover = isCover;
        filterDtoFile.FullName = Path.Combine(outputPath, filterDtoFile.FullName);

        var itemDtoFile = GenerateDto(entityInfo, DtoType.Item);
        itemDtoFile.IsCover = isCover;
        itemDtoFile.FullName = Path.Combine(outputPath, itemDtoFile.FullName);

        var detailDtoFile = GenerateDto(entityInfo, DtoType.Detail);
        detailDtoFile.IsCover = isCover;
        detailDtoFile.FullName = Path.Combine(outputPath, detailDtoFile.FullName);

        return
        [
            globalFile,
            addDtoFile,
            updateDtoFile,
            filterDtoFile,
            itemDtoFile,
            detailDtoFile
        ];
    }

    /// <summary>
    /// 生成manager的文件
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent">模板内容</param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateManager(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var managerGen = new ManagerGenerate(entityInfo);
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, managerGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };

        var content = managerGen.GetManagerContent(tplContent, entityInfo.GetManagerNamespace());
        var managerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Manager}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(outputPath, ConstVal.ManagersDir, $"{entityInfo.Name}{ConstVal.Manager}.cs"),
            ModuleName = entityInfo.ModuleName
        };

        var managerDIFile = GetManagerService(entityInfo, outputPath);
        return [globalFile, managerFile, managerDIFile];
    }

    /// <summary>
    /// Manager服务注入内容
    /// </summary>
    /// <returns></returns>
    public GenFileInfo GetManagerService(EntityInfo entityInfo, string outputPath)
    {
        var managerPath = Path.Combine(outputPath, ConstVal.ManagersDir);
        string content = ManagerGenerate.GetManagerServiceContent(managerPath, entityInfo.ModuleName);
        string name = entityInfo.ModuleName.IsEmpty()
            ? ConstVal.ManagerServiceExtensionsFile
            : ConstVal.ServiceExtensionsFile;

        return new GenFileInfo(name, content)
        {
            IsCover = true,
            FullName = Path.Combine(outputPath, name),
            ModuleName = entityInfo.ModuleName
        };
    }

    /// <summary>
    /// RestAPI生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent"></param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public GenFileInfo GenerateController(EntityInfo entityInfo, string outputPath, string tplContent, bool isCover = false)
    {
        var apiGen = new RestApiGenerate(entityInfo);
        var content = apiGen.GetRestApiContent(tplContent);
        var controllerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Controller}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(outputPath, $"{entityInfo.Name}{ConstVal.Controller}.cs"),
            ModuleName = entityInfo.ModuleName
        };
        return controllerFile;
    }

    public GenFileInfo GenerateApiGlobalUsing(EntityInfo entityInfo, string outputPath, bool isCover = false)
    {
        var apiGen = new RestApiGenerate(entityInfo);

        var globalFilePath = Path.Combine(outputPath, ConstVal.GlobalUsingsFile);
        var globalLines = File.Exists(globalFilePath)
            ? File.ReadLines(globalFilePath)
            : [];
        var globalList = apiGen.GetGlobalUsings();
        // add globalList  item if globalLines not exist 
        globalList.ForEach(g =>
        {
            if (!globalLines.Contains(g))
            {
                globalLines.Append(g);
            }
        });

        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, string.Join(Environment.NewLine, globalLines))
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName
        };
        return globalFile;
    }


    /// <summary>
    /// 生成Web请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="outputPath"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>> GenerateWebRequestAsync(string url = "", string outputPath = "", RequestLibType type = RequestLibType.NgHttp)
    {
        _logger.LogInformation("🚀 Generating ts models and {type} request services...", type);
        var files = new List<GenFileInfo>();

        // 1 parse openApi json from url
        string openApiContent = "";
        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            HttpClientHandler handler = new()
            {
                ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            };
            using HttpClient http = new(handler);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            openApiContent = await http.GetStringAsync(url);
            stopwatch.Stop();
            _logger.LogInformation("⬇️ Get OpenAPI from {url} {seconds} seconds", url, stopwatch.Elapsed.TotalSeconds);
        }
        else
        {
            openApiContent = File.ReadAllText(url);
        }
        openApiContent = openApiContent
            .Replace("«", "")
            .Replace("»", "");

        var apiDocument = new OpenApiStringReader().Read(openApiContent, out _);
        var docName = url.Contains("http")
            ? url.Split('/').Reverse().Skip(1).First()
            : string.Empty;

        // base service
        string content = RequestGenerate.GetBaseService(type);
        string dir = Path.Combine(outputPath, "services", docName);
        files.Add(new GenFileInfo("base.service.ts", content)
        {
            FullName = Path.Combine(dir, "base.service.ts"),
            IsCover = false
        });

        // 枚举pipe
        if (type == RequestLibType.NgHttp)
        {
            var schemas = apiDocument!.Components.Schemas;
            dir = Path.Combine(outputPath, "pipe", docName);
            var enumTextPath = Path.Combine(dir, "enum-text.pipe.ts");

            bool isNgModule = false;
            using (StreamReader reader = new(enumTextPath))
            {
                string? firstLine = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Contains("NgModule"))
                {
                    isNgModule = true;
                }
            }

            string pipeContent = RequestGenerate.GetEnumPipeContent(schemas, isNgModule);

            files.Add(new GenFileInfo("enum-text.pipe.ts", pipeContent)
            {
                FullName = enumTextPath,
                IsCover = true
            });
        }
        // request services
        var ngGen = new RequestGenerate(apiDocument!)
        {
            LibType = type
        };
        // 获取对应的ts模型类，生成文件
        var tsModels = ngGen.GetTSInterfaces();
        tsModels.ForEach(m =>
        {
            dir = Path.Combine(outputPath, "services", docName, m.FullName, "models");
            m.FullName = Path.Combine(dir, m.Name);
            m.IsCover = true;
        });
        files.AddRange(tsModels);
        // 获取请求服务并生成文件
        var services = ngGen.GetServices(apiDocument!.Tags);
        services.ForEach(s =>
        {
            dir = Path.Combine(outputPath, "services", docName, s.FullName);
            s.FullName = Path.Combine(dir, s.Name);
        });
        files.AddRange(services);
        return files;
    }

    /// <summary>
    /// 生成模板内容
    /// </summary>
    /// <param name="tplContent"></param>
    /// <param name="model"></param>
    /// <returns></returns>
    public string GenTemplateFile(string tplContent, ActionRunModel model)
    {
        var genContext = new RazorGenContext();
        try
        {
            return genContext.GenTemplate(tplContent, model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "🧐 Razor generate Error:{content}", tplContent);
            throw;
        }
    }

    /// <summary>
    /// 生成文件
    /// </summary>
    /// <param name="files"></param>
    public void GenerateFiles(List<GenFileInfo>? files)
    {
        if (files == null || files.Count == 0)
        {
            return;
        }
        foreach (var file in files)
        {
            if (file.IsCover || !File.Exists(file.FullName))
            {
                var dir = Path.GetDirectoryName(file.FullName);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir!);
                }
                File.WriteAllText(file.FullName, file.Content, Encoding.UTF8);
                _logger.LogInformation("🆕📄 :{path}", file.FullName);
            }
        }
    }

    public GenFileInfo GenerateDto(EntityInfo entityInfo, DtoType dtoType)
    {
        // 生成Dto
        var dtoGen = new DtoCodeGenerate(entityInfo);
        var dirName = entityInfo.Name + "Dtos";

        var dto = dtoType switch
        {
            DtoType.Add => dtoGen.GetAddDto(),
            DtoType.Update => dtoGen.GetUpdateDto(),
            DtoType.Filter => dtoGen.GetFilterDto(),
            DtoType.Item => dtoGen.GetItemDto(),
            DtoType.Detail => dtoGen.GetDetailDto(),
            _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, null)
        };

        //var md5Hash = HashCrypto.Md5Hash(dto.EntityNamespace + dto.Name);
        //var oldDto = await context.EntityInfos.Where(e => e.Md5Hash == md5Hash)
        //    .Include(e => e.PropertyInfos)
        //    .SingleOrDefaultAsync();

        //if (oldDto != null)
        //{
        //    var diff = PropertyInfo.GetDiffProperties(oldDto.PropertyInfos, dto.Properties);
        //    if (diff.delete.Count > 0)
        //    {
        //        dto.Properties = dto.Properties.Except(diff.delete).ToList();
        //    }
        //    if (diff.add.Count > 0)
        //    {
        //        dto.Properties.AddRange(diff.add);
        //    }
        //    context.PropertyInfos.RemoveRange(oldDto.PropertyInfos);
        //    dto.Properties.ForEach(p =>
        //    {
        //        p.EntityInfoId = oldDto.Id;
        //    });
        //    context.AddRange(dto.Properties);
        //}
        //else
        //{
        //    var newDto = dto.ToEntityInfo(entityInfo);
        //    context.EntityInfos.Add(newDto);
        //}
        //await context.SaveChangesAsync();
        var content = dto.ToDtoContent(entityInfo.GetDtoNamespace(), entityInfo.Name);

        return new GenFileInfo($"{dto.Name}.cs", content)
        {
            FullName = Path.Combine(ConstVal.ModelsDir, dirName, $"{dto.Name}.cs"),
            ModuleName = entityInfo.ModuleName
        };
    }
}

public enum DtoType
{
    [Description("Add")]
    Add,
    [Description("Update")]
    Update,
    [Description("Filter")]
    Filter,
    [Description("Item")]
    Item,
    [Description("Detail")]
    Detail
}