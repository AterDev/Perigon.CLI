using System.Diagnostics;
using CodeGenerator;
using CodeGenerator.Helper;
using Entity;
using Humanizer;
using Share.Models.CommandDtos;

namespace Share.Services;

/// <summary>
/// 解决方案相关功能
/// </summary>
public class SolutionService(
    IProjectContext projectContext,
    ILogger<SolutionService> logger,
    CommandDbContext context
)
{
    private readonly IProjectContext _projectContext = projectContext;
    private readonly ILogger<SolutionService> _logger = logger;
    private readonly CommandDbContext _context = context;

    /// <summary>
    /// 创建模块
    /// </summary>
    public async Task CreateModuleAsync(string moduleName)
    {
        string moduleDir = Path.Combine(_projectContext.SolutionPath!, PathConst.ModulesPath);
        if (!Directory.Exists(moduleDir))
        {
            Directory.CreateDirectory(moduleDir);
        }
        if (Directory.Exists(Path.Combine(moduleDir, moduleName)))
        {
            _logger.LogInformation("⚠️ 该模块已存在");
            return;
        }

        // 基础类
        string projectPath = Path.Combine(moduleDir, moduleName);
        _logger.LogInformation(
            "🚀 create module:{moduleName} ➡️ {projectPath}",
            moduleName,
            projectPath
        );

        // global usings
        string usingsContent = TplContent.ModuleGlobalUsings(moduleName);
        usingsContent = usingsContent.Replace("${Module}", moduleName);
        await AssemblyHelper.GenerateFileAsync(
            projectPath,
            ConstVal.GlobalUsingsFile,
            usingsContent,
            true
        );

        // project file
        string targetVersion = ConstVal.NetVersion;
        var csprojFiles = Directory
            .GetFiles(
                _projectContext.ApiPath!,
                $"*{ConstVal.CSharpProjectExtension}",
                SearchOption.TopDirectoryOnly
            )
            .FirstOrDefault();
        if (csprojFiles != null)
        {
            targetVersion = AssemblyHelper.GetTargetFramework(csprojFiles) ?? ConstVal.NetVersion;
        }
        string csprojContent = TplContent.DefaultModuleCSProject(targetVersion);
        await AssemblyHelper.GenerateFileAsync(
            projectPath,
            $"{moduleName}{ConstVal.CSharpProjectExtension}",
            csprojContent
        );

        // create dirs
        Directory.CreateDirectory(Path.Combine(projectPath, ConstVal.ModelsDir));
        Directory.CreateDirectory(Path.Combine(projectPath, ConstVal.ManagersDir));
        Directory.CreateDirectory(Path.Combine(projectPath, ConstVal.ControllersDir));

        //await AssemblyHelper.GenerateFileAsync(projectPath, "InitModule.cs", GetInitModuleContent(moduleName));

        await AddModuleConstFieldAsync(moduleName);
        // update solution file
        UpdateSolutionFile(
            Path.Combine(projectPath, $"{moduleName}{ConstVal.CSharpProjectExtension}")
        );
    }

    /// <summary>
    /// 添加模块常量
    /// </summary>
    public async Task AddModuleConstFieldAsync(string moduleName)
    {
        string moduleConstPath = Path.Combine(_projectContext.EntityPath!, "Modules.cs");
        if (File.Exists(moduleConstPath))
        {
            var entityPath = _projectContext.EntityPath;
            if (entityPath != null)
            {
                CompilationHelper analyzer = new(entityPath);
                string content = File.ReadAllText(moduleConstPath);
                analyzer.LoadContent(content);
                string fieldName = moduleName.Replace("Mod", "");

                if (!analyzer.FieldExist(fieldName))
                {
                    string newField = @$"public const string {fieldName} = ""{moduleName}"";";
                    analyzer.AddClassField(newField);
                    content = analyzer.SyntaxRoot!.ToFullString();
                    await AssemblyHelper.GenerateFileAsync(moduleConstPath, content, true);
                }
            }
        }
    }

    /// <summary>
    /// clean solution
    /// </summary>
    /// <param name="errorMsg"></param>
    /// <returns></returns>
    public bool CleanSolution(out string errorMsg)
    {
        errorMsg = string.Empty;
        string?[] dirPaths =
        [
            _projectContext.ServicesPath,
            _projectContext.EntityPath,
            _projectContext.EntityFrameworkPath,
            _projectContext.CommonModPath,
            _projectContext.SharePath,
            _projectContext.ModulesPath,
        ];

        string[] dirs = [];
        foreach (var path in dirPaths.Where(p => p.NotEmpty()))
        {
            if (!Directory.Exists(path))
            {
                continue;
            }
            dirs = dirs.Union(Directory.GetDirectories(path, "bin", SearchOption.TopDirectoryOnly))
                .Union(Directory.GetDirectories(path, "obj", SearchOption.TopDirectoryOnly))
                .ToArray();
        }
        try
        {
            foreach (string dir in dirs)
            {
                Directory.Delete(dir, true);
            }
            string? apiFiePath = Directory
                .GetFiles(_projectContext.ApiPath!, "*.csproj", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();

            if (apiFiePath != null)
            {
                Console.WriteLine($"⛏️ build project:{apiFiePath}");
                Process process = Process.Start("dotnet", $"build {apiFiePath}");
                process.WaitForExit();
                // if process has error message
                if (process.ExitCode != 0)
                {
                    errorMsg = "project build failed！";
                    return false;
                }
                return true;
            }
            errorMsg = $"can't find {apiFiePath}, please build manually!";
            return false;
        }
        catch (Exception ex)
        {
            errorMsg = "project clean failed, please try to close the occupied program and retry.";
            Console.WriteLine($"❌ Clean solution occur error:{ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 项目配置保存
    /// </summary>
    /// <returns></returns>
    public async Task<bool> SaveSyncDataLocalAsync()
    {
        var actions = await _context.GenActions.AsNoTracking().ToListAsync();
        var steps = await _context.GenSteps.AsNoTracking().ToListAsync();
        var relation = await _context.GenActionGenSteps.AsNoTracking().ToListAsync();

        var data = new SyncModel
        {
            TemplateSync = new TemplateSync
            {
                GenActions = actions,
                GenSteps = steps,
                GenActionGenSteps = relation,
            },
        };

        try
        {
            var templatePath = Path.Combine(_projectContext.SolutionPath!, ConstVal.TemplateDir);
            if (!Directory.Exists(templatePath))
            {
                Directory.CreateDirectory(templatePath);
            }
            var filePath = Path.Combine(templatePath, ConstVal.SyncJson);
            var json = JsonSerializer.Serialize(data, ConstVal.DefaultJsonSerializerOptions);
            await File.WriteAllTextAsync(filePath, json);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("同步数据到本地失败！{message}", ex.Message);
            return false;
        }
    }

    public async Task<(bool res, string? message)> SyncDataFromLocalAsync()
    {
        var filePath = Path.Combine(
            _projectContext.SolutionPath!,
            ConstVal.TemplateDir,
            ConstVal.SyncJson
        );
        var projectId = _projectContext.ProjectId;

        if (!File.Exists(filePath))
        {
            return (false, "templates/sync.json 文件不存在，无法同步");
        }

        var data = File.ReadAllText(filePath);
        var model = JsonSerializer.Deserialize<SyncModel>(data);
        if (model == null)
        {
            return (false, "没有有效数据");
        }

        try
        {
            var actions = await _context
                .GenActions.Where(a => a.ProjectId == projectId)
                .Include(a => a.GenSteps)
                .ToListAsync();

            _context.RemoveRange(actions);
            await _context.SaveChangesAsync();

            //var steps = await _context.GenSteps.Where(a => a.ProjectId == projectId).ToListAsync();
            //var relation = await _context.GenActionGenSteps.ToListAsync();

            // 去重并添加
            var newActions = model.TemplateSync?.GenActions.ToList();
            var newSteps = model.TemplateSync?.GenSteps.ToList();
            var newRelation = model.TemplateSync?.GenActionGenSteps.ToList();

            if (newActions != null && newActions.Count > 0)
            {
                newActions.ForEach(a => a.ProjectId = projectId);
                await _context.GenActions.AddRangeAsync(newActions);
            }
            if (newSteps != null && newSteps.Count > 0)
            {
                newSteps.ForEach(a => a.ProjectId = projectId);
                await _context.GenSteps.AddRangeAsync(newSteps);
            }
            await _context.SaveChangesAsync();
            _context.ChangeTracker.Clear();
            if (newRelation != null && newRelation.Count > 0)
            {
                await _context.GenActionGenSteps.AddRangeAsync(newRelation);
            }
            await _context.SaveChangesAsync();
            // 新增数量
            return (
                true,
                $"新增数据：{newActions?.Count}个操作，{newSteps?.Count}个步骤，{newRelation?.Count}个关联"
            );
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }

    /// <summary>
    /// 获取模块列表
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <returns>file path</returns>
    public static List<string>? GetModulesPaths(string solutionPath)
    {
        string modulesPath = Path.Combine(solutionPath, "src", "Modules");
        if (!Directory.Exists(modulesPath))
        {
            return default;
        }
        List<string> files =
        [
            .. Directory.GetFiles(
                modulesPath,
                $"*{ConstVal.CSharpProjectExtension}",
                SearchOption.AllDirectories
            ),
        ];
        return files.Count != 0 ? files : default;
    }

    /// <summary>
    /// 使用 dotnet sln add
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="isRemove">remove</param>
    private void UpdateSolutionFile(string projectPath, bool isRemove = false)
    {
        FileInfo? slnFile = AssemblyHelper.GetSlnFile(
            new DirectoryInfo(_projectContext.SolutionPath!)
        );
        if (slnFile != null)
        {
            if (isRemove)
            {
                // 从解决方案中移除
                if (
                    !ProcessHelper.RunCommand(
                        "dotnet",
                        $"sln {slnFile.FullName} remove {projectPath}",
                        out string error
                    )
                )
                {
                    _logger.LogInformation("remove project ➡️ solution failed:" + error);
                }
                else
                {
                    _logger.LogInformation("✅ remove project ➡️ solution!");
                }
            }
            else
            {
                // 添加到解决方案
                if (
                    !ProcessHelper.RunCommand(
                        "dotnet",
                        $"sln {slnFile.FullName} add {projectPath}",
                        out string error
                    )
                )
                {
                    _logger.LogInformation("add project ➡️ solution failed:" + error);
                }
                else
                {
                    _logger.LogInformation("✅ add project ➡️ solution!");
                }
            }
        }
    }

    /// <summary>
    /// 添加默认模块
    /// </summary>
    public static void AddDefaultModule(string moduleName, string solutionPath)
    {
        var moduleNames = ModuleInfo.GetModules().Select(m => m.Value).ToList();
        if (!moduleNames.Contains(moduleName))
        {
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string sourcePath = Path.Combine(studioPath, "Modules", moduleName);
        if (!Directory.Exists(sourcePath))
        {
            return;
        }

        string modulePath = Path.Combine(solutionPath, PathConst.ModulesPath, moduleName);
        string entityPath = Path.Combine(solutionPath, PathConst.EntityPath, moduleName);
        string databasePath = Path.Combine(solutionPath, PathConst.EntityFrameworkPath);

        // copy entities
        CopyModuleFiles(Path.Combine(sourcePath, "Entity"), entityPath);

        // copy module files
        CopyModuleFiles(sourcePath, modulePath);

        string dbContextFile = Path.Combine(databasePath, "DBProvider", "ContextBase.cs");
        string dbContextContent = File.ReadAllText(dbContextFile);

        CompilationHelper compilation = new(databasePath);
        compilation.LoadContent(dbContextContent);

        List<FileInfo> entityFiles = new DirectoryInfo(Path.Combine(sourcePath, "Entity"))
            .GetFiles("*.cs", SearchOption.AllDirectories)
            .ToList();

        entityFiles.ForEach(file =>
        {
            string entityName = Path.GetFileNameWithoutExtension(file.Name);
            var plural = entityName.Pluralize();
            string propertyString = $@"public DbSet<{entityName}> {plural} {{ get; set; }}";

            if (!compilation.PropertyExist(plural))
            {
                compilation.AddClassProperty(propertyString);
            }
        });

        dbContextContent = compilation.SyntaxRoot!.ToFullString();
        File.WriteAllText(dbContextFile, dbContextContent);
        // update globalUsings.cs
        string globalUsingsFile = Path.Combine(databasePath, "GlobalUsings.cs");
        string globalUsingsContent = File.ReadAllText(globalUsingsFile);

        string newLine = @$"global using Entity.{moduleName};";
        if (!globalUsingsContent.Contains(newLine))
        {
            globalUsingsContent = globalUsingsContent.Replace(
                "global using Entity;",
                $"global using Entity;{Environment.NewLine}{newLine}"
            );
            File.WriteAllText(globalUsingsFile, globalUsingsContent);
        }
    }

    /// <summary>
    /// 实现创建服务的逻辑
    /// </summary>
    /// <param name="serviceName"></param>
    /// <returns></returns>
    public async Task<(bool, string? errorMsg)> CreateServiceAsync(string serviceName)
    {
        var existService = GetServices().FirstOrDefault();

        var serviceDir = Path.Combine(_projectContext.ServicesPath!, serviceName);
        if (Directory.Exists(serviceDir))
        {
            return (false, "exist service");
        }
        else
        {
            Directory.CreateDirectory(serviceDir);
            var csprojContent = TplContent.ServiceProjectFileTpl(ConstVal.NetVersion);
            var csprojPath = Path.Combine(
                serviceDir,
                $"{serviceName}{ConstVal.CSharpProjectExtension}"
            );

            var programContent = TplContent.ServiceProgramTpl();
            var programPath = Path.Combine(serviceDir, "Program.cs");

            var globalUsingsContent = TplContent.ServiceGlobalUsingsTpl(serviceName);
            var globalUsingsPath = Path.Combine(serviceDir, ConstVal.GlobalUsingsFile);

            var launchSettingsContent = TplContent.ServiceLaunchSettingsTpl(serviceName);
            var launchSettingsPath = Path.Combine(serviceDir, "Propeties", "launchSettings.json");

            try
            {
                await AssemblyHelper.GenerateFileAsync(csprojPath, csprojContent);
                await AssemblyHelper.GenerateFileAsync(programPath, programContent);
                await AssemblyHelper.GenerateFileAsync(globalUsingsPath, globalUsingsContent);
                await AssemblyHelper.GenerateFileAsync(launchSettingsPath, launchSettingsContent);

                if (existService != null)
                {
                    var existAppSettingsPath = Path.Combine(
                        Path.GetDirectoryName(existService.Path)!,
                        ConstVal.AppSettingJson
                    );
                    var appSettingsPath = Path.Combine(serviceDir, ConstVal.AppSettingJson);
                    var appSettingsContent = "{}";
                    if (File.Exists(existAppSettingsPath))
                    {
                        appSettingsContent = await File.ReadAllTextAsync(existAppSettingsPath);
                    }
                    await AssemblyHelper.GenerateFileAsync(appSettingsPath, appSettingsContent);

                    // appsetings.development.json
                    var existAppSettingsDevPath = Path.Combine(
                        Path.GetDirectoryName(existService.Path)!,
                        ConstVal.AppSettingDevelopmentJson
                    );
                    var appSettingsDevPath = Path.Combine(
                        serviceDir,
                        ConstVal.AppSettingDevelopmentJson
                    );
                    var appSettingsDevContent = "{}";
                    if (File.Exists(existAppSettingsDevPath))
                    {
                        appSettingsDevContent = await File.ReadAllTextAsync(
                            existAppSettingsDevPath
                        );
                    }
                    await AssemblyHelper.GenerateFileAsync(
                        appSettingsDevPath,
                        appSettingsDevContent
                    );
                }
                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }
    }

    public List<SubProjectInfo> GetServices(bool onlyWeb = true)
    {
        List<SubProjectInfo> res = [];
        if (!Directory.Exists(_projectContext.ServicesPath!))
        {
            return [];
        }
        var projectFiles =
            Directory
                .GetFiles(
                    _projectContext.ServicesPath!,
                    $"*{ConstVal.CSharpProjectExtension}",
                    SearchOption.AllDirectories
                )
                .ToList() ?? [];

        projectFiles.ForEach(path =>
        {
            // 读取项目文件前三行内容
            string? content = null;
            content = string.Join(Environment.NewLine, File.ReadLines(path).Take(10));
            if (content != null)
            {
                SubProjectInfo moduleInfo = new()
                {
                    Name = Path.GetFileNameWithoutExtension(path),
                    Path = path,
                    ProjectType = ProjectType.WebAPI,
                };
                if (content.Contains("<Project Sdk=\"Microsoft.NET.Sdk.Web\">"))
                {
                    moduleInfo.ProjectType = ProjectType.WebAPI;
                }
                else if (content.Contains("<Project Sdk=\"Microsoft.NET.Sdk.Worker\">"))
                {
                    moduleInfo.ProjectType = ProjectType.Worker;
                }
                else if (content.Contains("<Project Sdk=\"Microsoft.NET.Sdk\">"))
                {
                    moduleInfo.ProjectType = ProjectType.Lib;
                    if (content.Contains("<OutputType>Exe</OutputType>"))
                    {
                        moduleInfo.ProjectType = ProjectType.Console;
                    }
                }
                res.Add(moduleInfo);
            }
        });
        if (onlyWeb)
        {
            return res.Where(m =>
                    m.ProjectType == ProjectType.WebAPI || m.ProjectType == ProjectType.Web
                )
                .ToList();
        }
        return res;
    }

    /// <summary>
    /// 复制模块文件
    /// </summary>
    /// <param name="sourceDir"></param>`
    /// <param name="destinationDir"></param>
    private static void CopyModuleFiles(string sourceDir, string destinationDir)
    {
        DirectoryInfo dir = new(sourceDir);
        if (!dir.Exists)
        {
            return;
        }

        DirectoryInfo[] dirs = dir.GetDirectories();
        Directory.CreateDirectory(destinationDir);

        // 获取源目录中的文件并复制到目标目录
        foreach (FileInfo file in dir.GetFiles())
        {
            string targetFilePath = Path.Combine(destinationDir, file.Name);
            file.CopyTo(targetFilePath, true);
        }

        foreach (DirectoryInfo subDir in dirs)
        {
            // 过滤不必要的目录
            if (subDir.Name is ConstVal.EntityName)
            {
                continue;
            }
            string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
            CopyModuleFiles(subDir.FullName, newDestinationDir);
        }
    }

    /// <summary>
    /// delete module
    /// </summary>
    /// <param name="moduleName"></param>
    public void DeleteModule(string moduleName)
    {
        string moduleDir = Path.Combine(_projectContext.SolutionPath!, PathConst.ModulesPath);
        var modulePath = Path.Combine(moduleDir, moduleName);
        if (Directory.Exists(modulePath))
        {
            Directory.Delete(modulePath, true);
        }
        UpdateSolutionFile(
            Path.Combine(modulePath, $"{moduleName}{ConstVal.CSharpProjectExtension}"),
            true
        );
    }
}
