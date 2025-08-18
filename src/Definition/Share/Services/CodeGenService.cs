using System.ComponentModel;
using CodeGenerator;
using CodeGenerator.Generate;
using CodeGenerator.Models;
using Entity;
using Microsoft.OpenApi;

namespace Share.Services;

/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService(ILogger<CodeGenService> logger, IProjectContext projectContext)
{
    private readonly ILogger<CodeGenService> _logger = logger;
    private readonly IProjectContext _projectContext = projectContext;

    /// <summary>
    /// 生成Dto
    /// </summary>
    /// <param name="entityInfo">实体信息</param>
    /// <param name="outputPath">输出项目目录</param>
    /// <param name="isCover">是否覆盖</param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateDtos(
        EntityInfo entityInfo,
        string outputPath,
        bool isCover = false
    )
    {
        _logger.LogInformation("🚀 Generating Dtos...");
        var dtoGen = new DtoCodeGenerate(entityInfo, _projectContext.SolutionConfig?.UserEntities);
        var dirName = entityInfo.Name + "Dtos";
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, dtoGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName,
        };

        // 统一生成各类型Dto文件
        var dtoTypes = new[]
        {
            DtoType.Add,
            DtoType.Update,
            DtoType.Filter,
            DtoType.Item,
            DtoType.Detail,
        };
        var dtoFiles = dtoTypes
            .Select(type =>
            {
                var file = GenerateDto(dtoGen, entityInfo, type);
                file.IsCover = isCover;
                file.FullName = Path.Combine(outputPath, file.FullName);
                return file;
            })
            .ToList();

        dtoFiles.Insert(0, globalFile);
        return dtoFiles;
    }

    /// <summary>
    /// 生成单个Dto（优化：接收DtoCodeGenerate实例，避免重复创建）
    /// </summary>
    /// <param name="dtoGen">DtoCodeGenerate实例</param>
    /// <param name="entityInfo"></param>
    /// <param name="dtoType"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public GenFileInfo GenerateDto(DtoCodeGenerate dtoGen, EntityInfo entityInfo, DtoType dtoType)
    {
        var dirName = entityInfo.Name + "Dtos";
        var dto = dtoType switch
        {
            DtoType.Add => dtoGen.GetAddDto(),
            DtoType.Update => dtoGen.GetUpdateDto(),
            DtoType.Filter => dtoGen.GetFilterDto(),
            DtoType.Item => dtoGen.GetItemDto(),
            DtoType.Detail => dtoGen.GetDetailDto(),
            _ => throw new ArgumentOutOfRangeException(nameof(dtoType), dtoType, null),
        };
        var content = dto.ToDtoContent(entityInfo.GetDtoNamespace(), entityInfo.Name);
        return new GenFileInfo($"{dto.Name}.cs", content)
        {
            FullName = Path.Combine(ConstVal.ModelsDir, dirName, $"{dto.Name}.cs"),
            ModuleName = entityInfo.ModuleName,
        };
    }

    /// <summary>
    /// 生成manager的文件
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="outputPath"></param>
    /// <param name="tplContent">模板内容</param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public List<GenFileInfo> GenerateManager(
        EntityInfo entityInfo,
        string outputPath,
        string tplContent,
        bool isCover = false
    )
    {
        var managerGen = new ManagerGenerate(
            entityInfo,
            _projectContext.SolutionConfig?.UserEntities ?? []
        );
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, managerGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName,
        };

        var content = managerGen.GetManagerContent(tplContent, entityInfo.GetCommonNamespace());
        var managerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Manager}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(
                outputPath,
                ConstVal.ManagersDir,
                $"{entityInfo.Name}{ConstVal.Manager}.cs"
            ),
            ModuleName = entityInfo.ModuleName,
        };

        return [globalFile, managerFile];
    }

    /// <summary>
    /// RestAPI生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="servicePath"></param>
    /// <param name="tplContent"></param>
    /// <param name="isCover"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>> GenerateControllerAsync(
        EntityInfo entityInfo,
        string servicePath,
        string tplContent,
        bool isCover = false
    )
    {
        var serviceName = Path.GetFileName(servicePath);
        var projectFile = Path.Combine(servicePath, serviceName + ConstVal.CSharpProjectExtension);

        // weather is management project
        var hasSystemMod = await SolutionService.HasProjectReferenceAsync(
            projectFile,
            _projectContext.SolutionConfig!.SystemModName
        );

        var apiGen = new RestApiGenerate(entityInfo, serviceName);
        var content = apiGen.GetRestApiContent(tplContent, hasSystemMod);
        var controllerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Controller}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(
                servicePath,
                ConstVal.ControllersDir,
                $"{entityInfo.Name}{ConstVal.Controller}.cs"
            ),
            ModuleName = entityInfo.ModuleName,
        };

        // global usings
        var globalFilePath = Path.Combine(servicePath, ConstVal.GlobalUsingsFile);
        var globalLines = File.Exists(globalFilePath)
            ? File.ReadLines(globalFilePath).ToList()
            : [];
        var globalList = apiGen.GetGlobalUsings();

        globalList.ForEach(g =>
        {
            if (!globalLines.Contains(g))
            {
                globalLines.Add(g);
            }
        });

        var globalFile = new GenFileInfo(
            ConstVal.GlobalUsingsFile,
            string.Join(Environment.NewLine, globalLines)
        )
        {
            IsCover = isCover,
            FileType = GenFileType.Global,
            FullName = Path.Combine(servicePath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName,
        };

        return [globalFile, controllerFile];
    }

    /// <summary>
    /// 生成Web请求
    /// </summary>
    /// <param name="url"></param>
    /// <param name="outputPath"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>?> GenerateWebRequestAsync(
        string url = "",
        string outputPath = "",
        RequestClientType type = RequestClientType.NgHttp
    )
    {
        _logger.LogInformation("🚀 Generating ts models and {type} request services...", type);
        var files = new List<GenFileInfo>();

        string docName = string.Empty;
        var (apiDocument, _) = await OpenApiDocument.LoadAsync(url);
        if (apiDocument == null)
        {
            OutputHelper.Error($"OpenApi document is parsed failed: {url}");
            return null;
        }

        // base service
        string content = RequestGenerate.GetBaseService(type);
        string dir = Path.Combine(outputPath, "services", docName);
        Directory.CreateDirectory(dir);
        files.Add(
            new GenFileInfo("base.service.ts", content)
            {
                FullName = Path.Combine(dir, "base.service.ts"),
                IsCover = false,
            }
        );

        // 枚举pipe
        if (type == RequestClientType.NgHttp)
        {
            var schemas = apiDocument.Components?.Schemas;
            if (schemas != null)
            {
                dir = Path.Combine(outputPath, "pipe", docName);
                Directory.CreateDirectory(dir);
                var enumTextPath = Path.Combine(dir, "enum-text.pipe.ts");
                bool isNgModule = false;
                if (File.Exists(enumTextPath))
                {
                    using (StreamReader reader = new(enumTextPath))
                    {
                        string? firstLine = reader.ReadLine();
                        string? secondLine = reader.ReadLine();
                        if (!string.IsNullOrWhiteSpace(firstLine) && firstLine.Contains("NgModule"))
                        {
                            isNgModule = true;
                        }

                        if (
                            !string.IsNullOrWhiteSpace(secondLine)
                            && secondLine.Contains("NgModule")
                        )
                        {
                            isNgModule = true;
                        }
                    }
                }
                string pipeContent = RequestGenerate.GetEnumPipeContent(schemas, isNgModule);

                files.Add(
                    new GenFileInfo("enum-text.pipe.ts", pipeContent)
                    {
                        FullName = enumTextPath,
                        IsCover = true,
                    }
                );
            }
        }
        // request services
        var ngGen = new RequestGenerate(apiDocument!) { LibType = type };
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
        if (apiDocument.Tags != null)
        {
            var services = ngGen.GetServices(apiDocument.Tags);
            services.ForEach(s =>
            {
                dir = Path.Combine(outputPath, "services", docName, s.FullName);
                s.FullName = Path.Combine(dir, s.Name);
            });
            files.AddRange(services);
        }
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
        var sb = new StringBuilder();
        sb.AppendLine();
        foreach (var file in files)
        {
            if (file.IsCover || !File.Exists(file.FullName))
            {
                var dir = Path.GetDirectoryName(file.FullName);
                if (Directory.Exists(dir) == false)
                {
                    Directory.CreateDirectory(dir!);
                }
                // 换行合并处理
                if (file.FileType == GenFileType.Global)
                {
                    var globalLines = File.Exists(file.FullName)
                        ? File.ReadLines(file.FullName)
                        : [];

                    var newLines = file.Content.Split(
                        new[] { Environment.NewLine },
                        StringSplitOptions.RemoveEmptyEntries
                    );

                    var lines = globalLines.ToList();
                    foreach (var line in newLines)
                    {
                        if (!lines.Contains(line))
                        {
                            lines.Add(line);
                        }
                    }
                    File.WriteAllLines(file.FullName, lines, new UTF8Encoding(false));
                }
                else
                {
                    File.WriteAllText(file.FullName, file.Content, new UTF8Encoding(false));
                }
                sb.AppendLine(file.FullName);
            }
        }
        _logger.LogInformation("🆕[files]: {path}", sb.ToString());
    }

    /// <summary>
    /// 生成Csharp请求客户端类库
    /// </summary>
    /// <param name="docUrl"></param>
    /// <param name="outputPath"></param>
    /// <returns></returns>
    public static async Task<List<GenFileInfo>> GenerateCsharpApiClientAsync(
        string docUrl,
        string outputPath
    )
    {
        var files = new List<GenFileInfo>();
        var docName = docUrl.Split('/').Reverse().Skip(1).First();
        var projectName = docName.ToPascalCase() + "API";
        outputPath = Path.Combine(outputPath, projectName);

        var (apiDocument, _) = await OpenApiDocument.LoadAsync(docUrl);
        var gen = new CSHttpClientGenerate(apiDocument!);

        string nspName = new DirectoryInfo(outputPath).Name;
        string baseContent = CSHttpClientGenerate.GetBaseService(nspName);
        string globalUsingContent = CSHttpClientGenerate.GetGlobalUsing(projectName);

        files.Add(
            new GenFileInfo("BaseService", baseContent)
            {
                FullName = Path.Combine(outputPath, "Services", "BaseService.cs"),
                IsCover = true,
            }
        );

        files.Add(
            new GenFileInfo("GlobalUsings", globalUsingContent)
            {
                FullName = Path.Combine(outputPath, "GlobalUsings.cs"),
                IsCover = false,
            }
        );

        // services
        List<GenFileInfo> services = gen.GetServices(nspName);
        services.ForEach(s =>
        {
            s.FullName = Path.Combine(outputPath, "Services", s.Name);
            s.IsCover = true;
        });
        // models
        List<GenFileInfo> models = gen.GetModelFiles(nspName);
        models.ForEach(s =>
        {
            s.FullName = Path.Combine(outputPath, "Models", s.Name);
            s.IsCover = true;
        });
        // csproj
        var csprojContent = CSHttpClientGenerate.GetCsprojContent();
        files.Add(
            new GenFileInfo(projectName, csprojContent)
            {
                FullName = Path.Combine(outputPath, $"{projectName}.csproj"),
                IsCover = true,
            }
        );

        files.AddRange(services);
        files.AddRange(models);

        return files;
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
    Detail,
}
