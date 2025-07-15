using CodeGenerator;
using CodeGenerator.Models;
using Microsoft.CodeAnalysis;

namespace StudioMod.Managers;

public partial class EntityInfoManager(
    DataAccessContext<ModelInfo> dataContext,
    ILogger<EntityInfoManager> logger,
    CodeAnalysisService codeAnalysis,
    CodeGenService codeGenService,
    IProjectContext projectContext
) : ManagerBase<ModelInfo>(dataContext, logger)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly CodeAnalysisService _codeAnalysis = codeAnalysis;
    private readonly CodeGenService _codeGenService = codeGenService;

    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <param name="serviceName">服务名称</param>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(string entityPath)
    {
        List<EntityFile> entityFiles = [];
        try
        {
            var filePaths = CodeAnalysisService.GetEntityFilePaths(entityPath);

            if (filePaths.Count != 0)
            {
                entityFiles = _codeAnalysis.GetEntityFiles(_projectContext.EntityPath!, filePaths);
                // 排序
                entityFiles =
                [
                    .. entityFiles.OrderByDescending(e => e.ModuleName).ThenBy(e => e.Name),
                ];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            _logger.LogInformation(ex.Message);
            return entityFiles;
        }
        return entityFiles;
    }

    /// <summary>
    /// 获取实体对应的 dto
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <returns></returns>
    public List<EntityFile> GetDtos(string entityFilePath)
    {
        List<EntityFile> dtoFiles = [];
        var dtoPath = GetDtoPath(entityFilePath);
        if (dtoPath == null)
        {
            return dtoFiles;
        }
        // get files in directory
        List<string> filePaths =
        [
            .. Directory.GetFiles(dtoPath, "*.cs", SearchOption.AllDirectories),
        ];

        if (filePaths.Count != 0)
        {
            filePaths = filePaths.Where(f => !f.EndsWith(".g.cs")).ToList();

            foreach (string? path in filePaths)
            {
                FileInfo file = new(path);
                EntityFile item = new()
                {
                    Name = file.Name,
                    BaseDirPath = dtoPath,
                    FullName = file.FullName,
                    Content = File.ReadAllText(path),
                };

                dtoFiles.Add(item);
            }
        }
        return dtoFiles;
    }

    private string? GetDtoPath(string entityFilePath)
    {
        var entityFile = _codeAnalysis.GetEntityFile(_projectContext.EntityPath!, entityFilePath);
        return entityFile?.GetDtoPath(_projectContext);
    }

    /// <summary>
    /// 获取文件内容
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="isManager"></param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    public EntityFile? GetFileContent(string entityName, bool isManager, string? moduleName = null)
    {
        if (entityName.EndsWith(".cs"))
        {
            entityName = entityName.Replace(".cs", "");
        }
        var entityFile = new EntityFile
        {
            Name = entityName,
            FullName = entityName,
            ModuleName = moduleName,
        };

        string? filePath;
        if (isManager)
        {
            filePath = entityFile.GetManagerPath(_projectContext);
            filePath = Path.Combine(filePath, $"{entityName}Manager.cs");
        }
        else
        {
            string entityDir = Path.Combine(_projectContext.EntityPath!);
            filePath = Directory
                .GetFiles(entityDir, $"{entityName}.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
        if (filePath != null)
        {
            FileInfo file = new(filePath);
            return new EntityFile()
            {
                Name = file.Name,
                BaseDirPath = file.DirectoryName ?? "",
                FullName = file.FullName,
                Content = File.ReadAllText(filePath),
            };
        }
        return default;
    }

    /// <summary>
    /// 保存Dto内容
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="Content"></param>
    /// <returns></returns>
    public async Task<bool> UpdateDtoContentAsync(string filePath, string Content)
    {
        try
        {
            if (filePath != null)
            {
                await File.WriteAllTextAsync(filePath, Content, new UTF8Encoding(false));
                return true;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            return false;
        }
        return false;
    }

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<List<GenFileInfo>> GenerateAsync(GenerateDto dto)
    {
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"build {_projectContext.EntityPath}",
                out string error
            )
        )
        {
            _logger.LogError(error);
        }

        var helper = new EntityParseHelper(dto.EntityPath);
        var entityInfo = await helper.ParseEntityAsync();
        _ = entityInfo ?? throw new Exception("Parse entity failed!");

        _logger.LogInformation("✨ entity module：{moduleName}", entityInfo.ModuleName);
        if (string.IsNullOrWhiteSpace(entityInfo.ModuleName))
        {
            _logger.LogWarning("⚠️ Using default module when not found module");
            entityInfo.ModuleName = ConstVal.CommonMod;
        }
        ModuleName = entityInfo.ModuleName;

        string modulePath = _projectContext.GetModulePath(entityInfo.ModuleName);
        var files = new List<GenFileInfo>();

        switch (dto.CommandType)
        {
            case CommandType.Dto:
                files.AddRange(GenerateDtos(entityInfo, modulePath, dto.Force));
                break;
            case CommandType.Manager:
                files.AddRange(GenerateDtos(entityInfo, modulePath, dto.Force));
                files.AddRange(GenerateManagers(entityInfo, modulePath, dto.Force));
                break;
            case CommandType.API:
                files.AddRange(GenerateDtos(entityInfo, modulePath, dto.Force));
                files.AddRange(GenerateManagers(entityInfo, modulePath, dto.Force));
                files.AddRange(GenerateControllers(entityInfo, dto));
                break;
            default:
                break;
        }
        _codeGenService.GenerateFiles(files);
        return files;
    }

    private List<GenFileInfo> GenerateDtos(EntityInfo entityInfo, string modulePath, bool force)
    {
        return _codeGenService.GenerateDtos(entityInfo, modulePath, force);
    }

    private List<GenFileInfo> GenerateManagers(EntityInfo entityInfo, string modulePath, bool force)
    {
        var tplContent = TplContent.ManagerTpl();
        return _codeGenService.GenerateManager(entityInfo, modulePath, tplContent, force);
    }

    /// <summary>
    /// 控制器生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    private List<GenFileInfo> GenerateControllers(EntityInfo entityInfo, GenerateDto dto)
    {
        var files = new List<GenFileInfo>();
        bool onlyOneService = dto.ServicePath.Length == 1;
        foreach (var apiPath in dto.ServicePath)
        {
            var controllerPath = Path.Combine(apiPath, ConstVal.ControllersDir);
            _codeGenService.GenerateApiGlobalUsing(entityInfo, apiPath, true);
            var controllerType =
                _projectContext.Project?.Config.ControllerType ?? ControllerType.Both;

            if (onlyOneService)
            {
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        apiPath,
                        TplContent.ControllerTpl(false),
                        true
                    )
                );
            }
            else
            {
                files.AddRange(GenerateControllerByType(entityInfo, apiPath, controllerType));
            }
        }
        return files;
    }

    private List<GenFileInfo> GenerateControllerByType(
        EntityInfo entityInfo,
        string apiPath,
        ControllerType controllerType
    )
    {
        var files = new List<GenFileInfo>();

        switch (controllerType)
        {
            case ControllerType.Client:
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        apiPath,
                        TplContent.ControllerTpl(false),
                        true
                    )
                );
                break;
            case ControllerType.Admin:
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        Path.Combine(apiPath, "AdminControllers"),
                        TplContent.ControllerTpl(),
                        true
                    )
                );
                break;
            case ControllerType.Both:
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        apiPath,
                        TplContent.ControllerTpl(false),
                        true
                    )
                );
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        Path.Combine(apiPath, "AdminControllers"),
                        TplContent.ControllerTpl(),
                        true
                    )
                );
                break;
            default:
                files.Add(
                    _codeGenService.GenerateController(
                        entityInfo,
                        apiPath,
                        TplContent.ControllerTpl(false),
                        true
                    )
                );
                break;
        }

        // 多服务时只生成 Client 端 Controller
        files.Add(
            _codeGenService.GenerateController(
                entityInfo,
                apiPath,
                TplContent.ControllerTpl(false),
                true
            )
        );
        return files;
    }
}
