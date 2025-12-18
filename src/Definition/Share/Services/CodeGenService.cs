using System.Collections.ObjectModel;
using CodeGenerator;
using CodeGenerator.Generate;
using CodeGenerator.Generate.ClientRequest;
using Entity;
using Microsoft.OpenApi;

namespace Share.Services;

/// <summary>
/// 代码生成服务
/// </summary>
public class CodeGenService(
    ILogger<CodeGenService> logger,
    IProjectContext projectContext,
    CacheService cache
)
{
    private readonly ILogger<CodeGenService> _logger = logger;
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CacheService _cache = cache;
    private readonly DtoType[] DtoTypes =
    [
        DtoType.Add,
        DtoType.Update,
        DtoType.Filter,
        DtoType.Item,
        DtoType.Detail,
    ];

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
        var dtoGen = new DtoCodeGenerate(entityInfo, _projectContext.SolutionConfig?.UserIdKeys);
        var dirName = entityInfo.Name + "Dtos";
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, dtoGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = true,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName,
        };

        var dtoFiles = DtoTypes
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
    /// 生成单个Dto
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

        // 缓存Dto信息
        _cache.Set(entityInfo.Name + dtoType.ToString(), dto);
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
            _projectContext.SolutionConfig?.UserIdKeys ?? []
        );
        // GlobalUsing
        var globalContent = string.Join(Environment.NewLine, managerGen.GetGlobalUsings());
        var globalFile = new GenFileInfo(ConstVal.GlobalUsingsFile, globalContent)
        {
            IsCover = true,
            FileType = GenFileType.Global,
            FullName = Path.Combine(outputPath, ConstVal.GlobalUsingsFile),
            ModuleName = entityInfo.ModuleName,
        };

        var isMultiTenant = SolutionService.IsMultiTenant(_projectContext.SolutionPath!);
        var content = managerGen.GetManagerContent(tplContent, entityInfo.GetCommonNamespace(), isMultiTenant);
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

        // whether is management project
        var hasSystemMod = await SolutionService.HasProjectReferenceAsync(
            projectFile,
            _projectContext.SolutionConfig!.SystemModName
        );

        OutputHelper.Important(
            $"{serviceName} with {_projectContext.SolutionConfig!.SystemModName} hasSystemMod: {hasSystemMod}"
        );

        var apiGen = new RestApiGenerate(
            entityInfo,
            _projectContext.SolutionConfig,
            GetDtoCache(entityInfo)
        );
        var content = apiGen.GetRestApiContent(tplContent, serviceName, hasSystemMod);
        var controllerFile = new GenFileInfo($"{entityInfo.Name}{ConstVal.Controller}.cs", content)
        {
            IsCover = isCover,
            FullName = Path.Combine(
                servicePath,
                ConstVal.ControllersDir,
                entityInfo.ModuleName ?? "",
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
            IsCover = true,
            FileType = GenFileType.Global,
            FullName = globalFilePath,
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
    /// <param name="onlyModels"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>?> GenerateWebRequestAsync(
        string url = "",
        string outputPath = "",
        RequestClientType type = RequestClientType.NgHttp,
        bool onlyModels = false
    )
    {
        _logger.LogInformation("🚀 Generating ts models and {type} request services...", type);
        var files = new List<GenFileInfo>();

        var (apiDocument, _) = await OpenApiDocument.LoadAsync(url);
        if (apiDocument == null)
        {
            OutputHelper.Error($"OpenApi document is parsed failed: {url}");
            return null;
        }

        string clientName = GetClientName(apiDocument);
        string dir = Path.Combine(outputPath, "services", clientName);
        // base service
        if (!onlyModels)
        {
            string content = RequestClientHelper.GetBaseService(type);
            content = content.Replace("BASE_URL", clientName.ToUpper() + "_BASE_URL");

            Directory.CreateDirectory(dir);
            files.Add(new GenFileInfo("base.service.ts", content)
            {
                FullName = Path.Combine(dir, "base.service.ts"),
                IsCover = false,
            });
        }

        // 枚举pipe
        if (type == RequestClientType.NgHttp)
        {
            var schemas = apiDocument.Components?.Schemas;
            if (schemas != null)
            {
                dir = Path.Combine(outputPath, "pipe", clientName);
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
                string pipeContent = RequestClientHelper.GetEnumPipeContent(schemas, isNgModule);

                files.Add(
                    new GenFileInfo("enum-text.pipe.ts", pipeContent)
                    {
                        FullName = enumTextPath,
                        IsCover = true,
                    }
                );
            }
        }
        // delete old files
        var oldPath = Path.Combine(outputPath, "services", clientName);
        try
        {
            if (Directory.Exists(oldPath))
            {
                Directory.Delete(oldPath, true);
            }
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"⚠️ Delete old files failed: {ex.Message}");
        }

        ClientRequestBase client = type switch
        {
            RequestClientType.NgHttp => new AngularClient(apiDocument!),
            RequestClientType.Axios => new AxiosClient(apiDocument!),
            _ => new AngularClient(apiDocument!) // 默认
        };
        client.ParseSchemas();
        // 生成 TS 模型文件
        var tsModels = client.GenerateModelFiles();
        tsModels.ForEach(m =>
        {
            dir = Path.Combine(
                outputPath,
                "services",
                clientName,
                m.DirName
            );
            m.FullName = Path.Combine(dir, m.Name);
            m.IsCover = true;
        });
        files.AddRange(tsModels);
        // 获取请求服务并生成文件
        if (!onlyModels && apiDocument.Tags != null)
        {
            var services = client.GenerateServices(apiDocument.Tags, clientName);
            services.ForEach(s =>
            {
                dir = Path.Combine(
                    outputPath,
                    "services",
                    clientName,
                    s.DirName
                );
                s.FullName = Path.Combine(dir, s.Name);
            });
            files.AddRange(services);
        }
        return files;
    }

    /// <summary>
    /// 生成Csharp请求客户端类库
    /// </summary>
    /// <param name="docUrl"></param>
    /// <param name="outputPath"></param>
    /// <param name="onlyModels"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>> GenerateCsharpApiClientAsync(
        string docUrl,
        string outputPath,
        bool onlyModels = false
    )
    {
        var files = new List<GenFileInfo>();
        var (apiDocument, _) = await OpenApiDocument.LoadAsync(docUrl);
        if (apiDocument == null) { return files; }

        var clientName = GetClientName(apiDocument);
        var projectName = clientName.ToPascalCase() + "Client";
        outputPath = Path.Combine(outputPath, projectName);

        var gen = new CSHttpClientGenerate(apiDocument!);
        string baseContent = CSHttpClientGenerate.GetBaseService(projectName);
        string globalUsingContent = CSHttpClientGenerate.GetGlobalUsing(projectName);


        files.Add(new GenFileInfo("GlobalUsings", globalUsingContent)
        {
            FullName = Path.Combine(outputPath, "GlobalUsings.cs"),
            IsCover = false,
        });

        if (!onlyModels)
        {
            files.Add(new GenFileInfo("BaseService", baseContent)
            {
                FullName = Path.Combine(outputPath, "Services", "BaseService.cs"),
                IsCover = true,
            });


            // services
            List<GenFileInfo> services = gen.GetServices(projectName);
            services.ForEach(s =>
            {
                s.FullName = Path.Combine(outputPath, "Services", s.Name);
                s.IsCover = true;
            });

            // extentions
            var extensionsContent = CSHttpClientGenerate.GetExtensionContent(
                projectName,
                services.Select(s => Path.GetFileNameWithoutExtension(s.Name)).ToList());
            files.Add(new GenFileInfo("Extensions", extensionsContent)
            {
                FullName = Path.Combine(outputPath, "Extensions.cs"),
                IsCover = true
            });
            files.AddRange(services);
        }

        // models
        List<GenFileInfo> models = gen.GetModelFiles(projectName);
        models.ForEach(s =>
        {
            s.FullName = Path.Combine(outputPath, "Models", s.DirName, s.Name);
            s.IsCover = true;
        });
        // csproj
        var csprojContent = CSHttpClientGenerate.GetCsprojContent(ConstVal.NetVersion);
        files.Add(
            new GenFileInfo(projectName, csprojContent)
            {
                FullName = Path.Combine(outputPath, $"{projectName}.csproj"),
                IsCover = true,
            }
        );

        files.AddRange(models);
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
    /// <param name="files">待生成的文件列表</param>
    /// <param name="output">是否写入到磁盘</param>
    public void GenerateFiles(List<GenFileInfo>? files, bool output = true)
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
        if (output)
        {
            _logger.LogInformation("🆕[files]: {path}", sb.ToString());
        }
    }


    public void ClearCodeGenCache(EntityInfo entityInfo)
    {

        foreach (var dtoType in DtoTypes)
        {
            var key = entityInfo.Name + dtoType.ToString();
            _cache.Remove(key);
        }
    }
    private static string GetClientName(OpenApiDocument apiDocument)
    {
        var clientName = string.Empty;
        var title = apiDocument.Info.Title;
        if (!string.IsNullOrWhiteSpace(title))
        {
            var matchName = title
                .Split('|')
                .Where(s => s.EndsWith("Service", StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            matchName ??= title.Split('|').FirstOrDefault();

            if (matchName != null)
            {
                clientName = matchName.Trim().Replace("Service", "").ToHyphen();
            }
        }

        return clientName;
    }
    /// <summary>
    /// get Dto from cache
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <returns></returns>
    private ReadOnlyDictionary<string, DtoInfo> GetDtoCache(EntityInfo entityInfo)
    {
        var result = new Dictionary<string, DtoInfo>();
        foreach (var dtoType in DtoTypes)
        {
            var key = entityInfo.Name + dtoType.ToString();
            var dto = _cache.Get<DtoInfo>(key);
            if (dto != null)
            {
                result.Add(dtoType.ToString(), dto);
            }
        }
        return new ReadOnlyDictionary<string, DtoInfo>(result);
    }

}
