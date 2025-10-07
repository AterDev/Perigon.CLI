using CodeGenerator;
using CodeGenerator.Models;
using Microsoft.CodeAnalysis;

namespace StudioMod.Managers;

public partial class EntityInfoManager(
    ILogger<EntityInfoManager> logger,
    CodeGenService codeGenService,
    IProjectContext projectContext
) : ManagerBase(logger)
{
    public string ModuleName { get; set; } = string.Empty;

    /// <summary>
    /// 获取实体列表
    /// </summary>
    /// <returns></returns>
    public List<EntityFile> GetEntityFiles(string entityPath, bool forceRefresh = false)
    {
        List<EntityFile> entityFiles = [];
        //try
        //{
        var entityProjectPath = Path.Combine(
            entityPath,
            ConstVal.EntityName + ConstVal.CSharpProjectExtension
        );
        if (File.Exists(entityProjectPath) && forceRefresh)
        {
            if (!SolutionService.BuildProject(entityProjectPath, true))
            {
                OutputHelper.Error($"build entity project: {entityProjectPath} failed.");
            }
        }

        var filePaths = CodeAnalysisService.GetEntityFilePaths(entityPath);
        if (filePaths.Count != 0)
        {
            entityFiles = CodeAnalysisService.GetEntityFiles(projectContext.EntityPath!, filePaths);
            // 排序
            entityFiles = [.. entityFiles.OrderByDescending(e => e.ModuleName).ThenBy(e => e.Name)];
        }
        //}
        //catch (Exception ex)
        //{
        //    _logger.LogInformation("{message}", ex.Message);
        //    return entityFiles;
        //}
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
        var entityFile = CodeAnalysisService.GetEntityFile(
            projectContext.EntityPath!,
            entityFilePath
        );
        return entityFile?.GetDtoPath(projectContext);
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
            filePath = entityFile.GetManagerPath(projectContext);
            filePath = Path.Combine(filePath, $"{entityName}Manager.cs");
        }
        else
        {
            string entityDir = Path.Combine(projectContext.EntityPath!);
            filePath = Directory
                .GetFiles(entityDir, $"{entityName}.cs", SearchOption.AllDirectories)
                .FirstOrDefault();
        }
        if (filePath != null)
        {
            System.IO.FileInfo file = new(filePath);
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
    public static async Task<bool> UpdateDtoContentAsync(string filePath, string Content)
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
        //SolutionService.BuildProject(projectContext.EntityPath!, false);
        SolutionService.BuildProject(projectContext.EntityFrameworkPath!, false);

        var dbContextHelper = new DbContextParseHelper(
            projectContext.EntityPath!,
            projectContext.EntityFrameworkPath!
        );
        var entityType = await dbContextHelper.LoadEntityAsync(dto.EntityPath);
        if (entityType == null)
        {
            throw new Exception($"Entity: {dto.EntityPath} Parse failed!");
        }
        var entityInfo = dbContextHelper.GetEntityInfo(entityType);
        _ = entityInfo ?? throw new Exception("Parse entity failed!");

        _logger.LogInformation("✨ entity module：{moduleName}", entityInfo.ModuleName);
        if (string.IsNullOrWhiteSpace(entityInfo.ModuleName))
        {
            _logger.LogWarning("⚠️ Using default module when not found module");
            entityInfo.ModuleName = ConstVal.CommonMod;
        }
        ModuleName = entityInfo.ModuleName;

        string modulePath = projectContext.GetModulePath(entityInfo.ModuleName);
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
                files.AddRange(await GenerateControllersAsync(entityInfo, dto));
                break;
            default:
                break;
        }
        codeGenService.ClearCodeGenCache(entityInfo);
        // 清理并释放资源
        entityInfo = null;
        dbContextHelper = null;
        GC.Collect();
        GC.WaitForPendingFinalizers();
        codeGenService.GenerateFiles(files);
        return files;
    }

    private List<GenFileInfo> GenerateDtos(EntityInfo entityInfo, string modulePath, bool force)
    {
        return codeGenService.GenerateDtos(entityInfo, modulePath, force);
    }

    private List<GenFileInfo> GenerateManagers(EntityInfo entityInfo, string modulePath, bool force)
    {
        var tplContent = TplContent.ManagerTpl();
        return codeGenService.GenerateManager(entityInfo, modulePath, tplContent, force);
    }

    /// <summary>
    /// 控制器生成
    /// </summary>
    /// <param name="entityInfo"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    private async Task<List<GenFileInfo>> GenerateControllersAsync(
        EntityInfo entityInfo,
        GenerateDto dto
    )
    {
        var files = new List<GenFileInfo>();
        foreach (var servicePath in dto.ServicePath)
        {
            var controllers = await codeGenService.GenerateControllerAsync(
                entityInfo,
                servicePath,
                TplContent.ControllerTpl(),
                dto.Force
            );
            files.AddRange(controllers);
            // add project Reference
            if (projectContext.ModulesPath.NotEmpty() && entityInfo.ModuleName.NotEmpty())
            {
                var moduleProjectPath = Path.Combine(
                    projectContext.ModulesPath,
                    entityInfo.ModuleName,
                    entityInfo.ModuleName + ConstVal.CSharpProjectExtension
                );

                var serviceProjectPath = Path.Combine(
                    servicePath,
                    Path.GetFileName(servicePath) + ConstVal.CSharpProjectExtension
                );
                SolutionService.AddProjectReference(serviceProjectPath, moduleProjectPath);
            }
        }
        return files;
    }
}
