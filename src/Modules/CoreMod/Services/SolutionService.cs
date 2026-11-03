using Humanizer;
using System.Diagnostics;

namespace CoreMod.Services;

/// <summary>
/// 解决方案相关功能
/// </summary>
public class SolutionService(
    SolutionContext projectContext,
    ILogger<SolutionService> logger,
    DefaultDbContext context
)
{
    private readonly SolutionContext _projectContext = projectContext;
    private readonly ILogger<SolutionService> _logger = logger;

    /// <summary>
    /// 保存解决方案
    /// </summary>
    /// <param name="solutionPath"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public async Task<int> SaveSolutionAsync(string solutionPath, string name)
    {
        var projectFilePath = Directory
            .GetFiles(solutionPath, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
        projectFilePath ??= Directory
            .GetFiles(
                solutionPath,
                $"*{ConstVal.SolutionXMLExtension}",
                SearchOption.TopDirectoryOnly
            )
            .FirstOrDefault();
        projectFilePath ??= Directory
            .GetFiles(
                solutionPath,
                $"*{ConstVal.CSharpProjectExtension}",
                SearchOption.TopDirectoryOnly
            )
            .FirstOrDefault();
        projectFilePath ??= Directory
            .GetFiles(solutionPath, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var solutionName = Path.GetFileName(projectFilePath) ?? name;
        var entity = new Solution()
        {
            DisplayName = name,
            Path = solutionPath,
            Name = solutionName,
            SolutionType = solutionType,
        };
        entity.Config.SolutionPath = solutionPath;
        context.Solutions.Add(entity);
        await context.SaveChangesAsync();
        return entity.Id;
    }

    /// <summary>
    /// 判断当前解决方案是否启用 AOT
    /// </summary>
    public bool IsAOT()
    {
        return IsAOT(_projectContext.SolutionPath);
    }

    /// <summary>
    /// 判断指定解决方案是否启用 AOT
    /// </summary>
    /// <param name="solutionPath">解决方案根目录</param>
    public static bool IsAOT(string? solutionPath)
    {
        if (solutionPath.NotEmpty())
        {
            string configPath = Path.Combine(solutionPath!, ".config", "perigon.config.toml");
            if (File.Exists(configPath))
            {
                foreach (string line in File.ReadLines(configPath))
                {
                    string trimmedLine = line.Split('#', 2)[0].Trim();
                    string[] parts = trimmedLine.Split('=', 2);
                    if (
                        parts.Length == 2
                        && parts[0].Trim().Equals("isAOT", StringComparison.OrdinalIgnoreCase)
                        && bool.TryParse(parts[1].Trim(), out bool isAot)
                    )
                    {
                        return isAot;
                    }
                }
            }
        }

        return false;
    }

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
        var csprojFile = Directory
            .GetFiles(
                _projectContext.ServicesPath!,
                $"*{ConstVal.CSharpProjectExtension}",
                SearchOption.AllDirectories
            )
            .FirstOrDefault();
        if (csprojFile != null)
        {
            targetVersion = AssemblyHelper.GetTargetFramework(csprojFile) ?? ConstVal.NetVersion;
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

        // extension and init files
        await AssemblyHelper.GenerateFileAsync(
            projectPath,
            ConstVal.ModuleExtensionFile,
            TplContent.ModuleExtension(moduleName)
        );
        await AssemblyHelper.GenerateFileAsync(
            Path.Combine(projectPath, ConstVal.ServicesDir),
           $"Init{moduleName}Service.cs",
           TplContent.ModuleInitHostService(moduleName)
       );

        // update solution file
        UpdateSolutionFile(
            Path.Combine(projectPath, $"{moduleName}{ConstVal.CSharpProjectExtension}")
        );
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

            Console.WriteLine($"⛏️ rebuild solution");
            Process process = Process.Start("dotnet", $"build {_projectContext.SolutionPath}");
            process.WaitForExit();
            // if process has error message
            if (process.ExitCode != 0)
            {
                errorMsg = "project build failed！";
                return false;
            }
            return true;
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
        var actions = context.GenActions.ToList();
        var steps = context.GenSteps.ToList();
        var relation = context.GenActionGenSteps.ToList();

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

    /// <summary>
    /// 使用 dotnet sln add
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="isRemove">remove</param>
    public void UpdateSolutionFile(string projectPath, bool isRemove = false)
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
                    _logger.LogInformation("remove project ➡️ solution failed:{error}", error);
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
                    _logger.LogInformation("add project ➡️ solution failed:{error}", error);
                }
                else
                {
                    _logger.LogInformation("✅ add project ➡️ solution!");
                }
            }
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
        var serviceFilePath = Path.Combine(
            serviceDir,
            $"{serviceName}{ConstVal.CSharpProjectExtension}"
        );
        if (File.Exists(serviceFilePath))
        {
            return (false, "💀 Exist service!");
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
            var launchSettingsPath = Path.Combine(serviceDir, "Properties", "launchSettings.json");

            try
            {
                await AssemblyHelper.GenerateFileAsync(csprojPath, csprojContent);
                await AssemblyHelper.GenerateFileAsync(programPath, programContent);
                await AssemblyHelper.GenerateFileAsync(globalUsingsPath, globalUsingsContent);
                await AssemblyHelper.GenerateFileAsync(launchSettingsPath, launchSettingsContent);

                // copy settings from other service
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

                    // appsettings.Development.json
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
                // add project to solution
                var projectPath = Path.Combine(
                    serviceDir,
                    $"{serviceName}{ConstVal.CSharpProjectExtension}"
                );
                UpdateSolutionFile(projectPath);

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
        return onlyWeb
            ? res.Where(m => m.ProjectType is ProjectType.WebAPI or ProjectType.Web).ToList()
            : res;
    }

    /// <summary>
    /// delete module
    /// </summary>
    /// <param name="moduleName"></param>
    public void DeleteModule(string moduleName)
    {
        OutputHelper.Important($"🏃‍♂️‍➡️ Deleting module: {moduleName}");
        // 1 获取并移除实体
        var entityDir = Path.Combine(
            _projectContext.SolutionPath!,
            PathConst.EntityPath,
            moduleName
        );
        List<string> entityNames = [];
        if (Directory.Exists(entityDir))
        {
            // 获取所有文件名作为实体名
            entityNames = Directory
                .GetFiles(entityDir, "*.cs")
                .Select(Path.GetFileNameWithoutExtension)
                .ToList()!;
            Directory.Delete(entityDir, true);
            OutputHelper.Success($"[1] remove entity dir: {entityDir}!");
        }
        // 2 移除DbContext中的DbSet
        if (entityNames.Count > 0)
        {
            var databasePath = _projectContext.EntityFrameworkPath!;
            var dbContextDir = Path.Combine(databasePath, ConstVal.AppDbContextName);
            if (Directory.Exists(dbContextDir))
            {
                var dbContextFiles = Directory.GetFiles(dbContextDir, "*.cs");
                foreach (var dbContextFile in dbContextFiles)
                {
                    var dbContextFileName = Path.GetFileName(dbContextFile);
                    if (!dbContextFile.EndsWith("DbContext.cs") || dbContextFile == ConstVal.ContextBase + ".cs")
                    {
                        continue;
                    }
                    var dbContextContent = File.ReadAllText(dbContextFile);
                    CompilationHelper compilation = new(databasePath);
                    compilation.LoadContent(dbContextContent);

                    if (
                        compilation.GetParentClassName() == ConstVal.ContextBase
                        || compilation.GetParentClassName() == "DbContext"
                    )
                    {
                        foreach (var entityName in entityNames)
                        {
                            var plural = entityName.Pluralize();
                            if (compilation.PropertyExist(plural))
                            {
                                compilation.RemoveClassProperty(plural);
                                OutputHelper.Success(
                                    $"[2] remove DbSet<{entityName}> {plural} from {dbContextFileName}!"
                                );
                            }
                            compilation.RemoveOnModelCreating(entityName);
                        }


                        dbContextContent = compilation.SyntaxRoot!.ToFullString();
                        // 命名空间移除
                        dbContextContent = dbContextContent.Replace($"using Entity.{moduleName};", "");
                        OutputHelper.Success($"[2] remove namesapce from {dbContextFileName}!");
                        File.WriteAllText(dbContextFile, dbContextContent);
                    }
                }
            }
        }


        var moduleDir = Path.Combine(_projectContext.SolutionPath!, PathConst.ModulesPath);
        var modulePath = Path.Combine(moduleDir, moduleName);
        var moduleFilePath = Path.Combine(
            modulePath,
            $"{moduleName}{ConstVal.CSharpProjectExtension}"
        );

        // 3. 服务中的项目引用
        if (entityNames.Count > 0 && Directory.Exists(_projectContext.ServicesPath!))
        {
            var serviceDirs = Directory.GetDirectories(_projectContext.ServicesPath!);
            foreach (var serviceDir in serviceDirs)
            {
                var controllersDir = Path.Combine(serviceDir, ConstVal.ControllersDir);
                if (Directory.Exists(controllersDir))
                {
                    foreach (var entityName in entityNames)
                    {
                        var controllerFile = Path.Combine(
                            controllersDir,
                            moduleName,
                            $"{entityName}{ConstVal.Controller}.cs"
                        );
                        if (File.Exists(controllerFile))
                        {
                            File.Delete(controllerFile);
                        }
                    }
                }
                var globalUsingsFile = Path.Combine(
                    serviceDir,
                    ConstVal.GlobalUsingsFile
                );
                if (File.Exists(globalUsingsFile))
                {
                    var lines = File.ReadAllLines(globalUsingsFile).ToList();
                    lines = lines.Where(line => !line.Contains($"{moduleName}")).ToList();
                    File.WriteAllLines(globalUsingsFile, lines);
                }
                // 移除引用
                var serviceProjectFile = Directory.GetFiles(
                        serviceDir,
                        $"*{ConstVal.CSharpProjectExtension}",
                        SearchOption.TopDirectoryOnly
                    ).FirstOrDefault();

                if (serviceProjectFile != null)
                {
                    if (ProcessHelper.RunCommand(
                        "dotnet",
                        $"remove {serviceProjectFile} reference {moduleFilePath}",
                        out string _
                    ))
                    {
                        OutputHelper.Success($"[3] remove project reference ➡️ {serviceProjectFile}!");
                    }
                }
            }
        }
        // 4. 从解决方案中移除
        UpdateSolutionFile(moduleFilePath, true);
        OutputHelper.Success($"[4] remove {moduleName} from solution");
        // 5. 删除模块目录
        if (Directory.Exists(modulePath))
        {
            Directory.Delete(modulePath, true);
            OutputHelper.Success($"[5] delte module dir: {modulePath}!");
        }
    }

    public string GenerateMigrations(string dbContextName, string migrationName)
    {
        var efPath = _projectContext.EntityFrameworkPath;
        if (!Directory.Exists(efPath))
        {
            return $"EntityFramework path {efPath} not exist!";
        }
        var migrationDir = Path.Combine(_projectContext.ServicesPath ?? "", "MigrationService");
        if (!Directory.Exists(migrationDir))
        {
            return $"MigrationService path {migrationDir} not exist!";
        }
        var commands = new string[]
        {
            $"cd {efPath}",
            $"dotnet build",
            $"dotnet ef migrations add --context {dbContextName} --project {efPath}",
        };
        var output = ProcessHelper.ExecuteCommands(commands);
        return output;
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
    /// 添加默认模块
    /// </summary>
    public static void AddDefaultModule(string moduleName, string solutionPath)
    {
        string studioPath = AssemblyHelper.GetStudioPath();
        string sourcePath = Path.Combine(studioPath, ConstVal.ModulesDir, moduleName);
        if (!Directory.Exists(sourcePath))
        {
            OutputHelper.Warning($"{sourcePath} not exist!");
            return;
        }

        string modulePath = Path.Combine(solutionPath, PathConst.ModulesPath, moduleName);
        string entityPath = Path.Combine(solutionPath, PathConst.EntityPath, moduleName);
        string databasePath = Path.Combine(solutionPath, PathConst.EntityFrameworkPath);

        // copy entities
        CopyModuleFiles(Path.Combine(sourcePath, ConstVal.EntityName), entityPath);

        // copy module files
        CopyModuleFiles(sourcePath, modulePath);

        string dbContextFile = Path.Combine(databasePath, ConstVal.AppDbContextName, "DefaultDbContext.cs");
        string dbContextContent = File.ReadAllText(dbContextFile);

        CompilationHelper compilation = new(databasePath);
        compilation.LoadContent(dbContextContent);

        List<FileInfo> entityFiles = new DirectoryInfo(
            Path.Combine(sourcePath, ConstVal.EntityName)
        )
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
        var slnFile = AssemblyHelper.GetSlnFile(new DirectoryInfo(solutionPath));

        var moduleProjectPath = Path.Combine(
            modulePath,
            $"{moduleName}{ConstVal.CSharpProjectExtension}"
        );

        if (slnFile == null || !File.Exists(moduleProjectPath))
        {
            OutputHelper.Warning(
                $"slnFile {slnFile} or module project file {moduleProjectPath} not found!"
            );
            return;
        }

        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"sln {slnFile.FullName} add {moduleProjectPath}",
                out string error
            )
        )
        {
            OutputHelper.Error("add project ➡️ solution failed:" + error);
        }
        else
        {
            OutputHelper.Success($"add project {moduleProjectPath} ➡️ solution!");
        }
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
        foreach (System.IO.FileInfo file in dir.GetFiles())
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
    /// 构建项目
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="restore"></param>
    /// <returns></returns>
    public static bool BuildProject(string projectPath, bool restore = true)
    {
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"build {projectPath} {(restore ? "" : "--no-restore")}",
                out string error
            )
        )
        {
            OutputHelper.Error(error);
            return false;
        }
        else
        {
            OutputHelper.Success(error);
            return true;
        }
    }

    public static async Task<bool> AddProjectReferenceAsync(string projectPath, string referencePath)
    {

        if (await HasProjectReferenceAsync(projectPath, referencePath))
        {
            return true;
        }
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"add {projectPath} reference {referencePath}",
                out string error
            )
        )
        {
            OutputHelper.Error($"add project reference {referencePath} failed!:{error}");
            return false;
        }
        return true;
    }

    /// <summary>
    /// determine if the project has a specific reference
    /// </summary>
    /// <param name="projectPath"></param>
    /// <param name="referenceName"></param>
    /// <returns></returns>
    public static async Task<bool> HasProjectReferenceAsync(
        string projectPath,
        string referenceName
    )
    {
        if (!File.Exists(projectPath))
        {
            return false;
        }
        var lines = await File.ReadAllLinesAsync(projectPath);

        var searchText = $"\\{referenceName}\\{referenceName}{ConstVal.CSharpProjectExtension}";
        return lines.Any(line => line.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsMultiTenant(string solutionPath)
    {
        bool isMultiTenant = false;
        var settingPath = Path.Combine(
            solutionPath,
            PathConst.AppHostPath,
            ConstVal.AppSettingDevelopmentJson
        );

        if (File.Exists(settingPath))
        {

            var content = File.ReadAllText(settingPath);
            if (content.Contains("\"IsMultiTenant\": true"))
            {
                isMultiTenant = true;
            }
        }
        return isMultiTenant;
    }

}
