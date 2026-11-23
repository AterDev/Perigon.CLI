using System.Xml;
using System.Xml.Linq;

namespace CoreMod.Services;

/// <summary>
/// 解决方案相关功能
/// </summary>
public class SolutionService(
    SolutionContext projectContext,
    ILogger<SolutionService> logger
)
{
    private readonly SolutionContext _projectContext = projectContext;
    private readonly ILogger<SolutionService> _logger = logger;

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
    /// 使用 dotnet sln add
    /// </summary>
    /// <param name="projectPath"></param>
    public bool UpdateSolutionFile(string projectPath)
    {
        FileInfo? slnFile = AssemblyHelper.GetSlnFile(
            new DirectoryInfo(_projectContext.SolutionPath!)
        );
        if (slnFile == null)
        {
            _logger.LogWarning("Solution file was not found below {SolutionPath}", _projectContext.SolutionPath);
            return false;
        }

        if (!File.Exists(projectPath))
        {
            _logger.LogWarning("Project file was not found: {ProjectPath}", projectPath);
            return false;
        }

        if (SolutionContainsProject(slnFile.FullName, projectPath))
        {
            _logger.LogDebug(
                "Project {ProjectPath} is already present in solution {SolutionPath}",
                projectPath,
                slnFile.FullName
            );
            return true;
        }

        var command = $"sln {QuoteCommandArgument(slnFile.FullName)} add --include-references false {QuoteCommandArgument(projectPath)}";
        if (!ProcessHelper.RunCommand("dotnet", command, out string error))
        {
            // Older SDKs do not expose --include-references. Retry the legacy command only for that
            // capability mismatch; all other errors must remain visible to the caller.
            if (error.Contains("include-references", StringComparison.OrdinalIgnoreCase))
            {
                var legacyCommand = $"sln {QuoteCommandArgument(slnFile.FullName)} add {QuoteCommandArgument(projectPath)}";
                if (ProcessHelper.RunCommand("dotnet", legacyCommand, out error))
                {
                    _logger.LogInformation("Project {ProjectPath} added to solution {SolutionPath}", projectPath, slnFile.FullName);
                    return true;
                }
            }

            _logger.LogWarning(
                "Failed to add project {ProjectPath} to solution {SolutionPath}: {Error}",
                projectPath,
                slnFile.FullName,
                error
            );
            return false;
        }

        _logger.LogInformation("Project {ProjectPath} added to solution {SolutionPath}", projectPath, slnFile.FullName);
        return true;
    }

    private static bool SolutionContainsProject(string solutionPath, string projectPath)
    {
        var fullProjectPath = Path.GetFullPath(projectPath);

        try
        {
            if (Path.GetExtension(solutionPath).Equals(ConstVal.SolutionXMLExtension, StringComparison.OrdinalIgnoreCase))
            {
                var document = XDocument.Load(solutionPath);
                return document
                    .Descendants()
                    .Where(element => element.Name.LocalName.Equals("Project", StringComparison.OrdinalIgnoreCase))
                    .Select(element => element.Attribute("Path")?.Value)
                    .Where(path => !string.IsNullOrWhiteSpace(path))
                    .Select(path => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(solutionPath)!, path!)))
                    .Any(path => PathsEqual(path, fullProjectPath));
            }

            foreach (var line in File.ReadLines(solutionPath))
            {
                var parts = line.Split('"');
                if (parts.Length < 4 || !line.TrimStart().StartsWith("Project(", StringComparison.Ordinal))
                {
                    continue;
                }

                var solutionProjectPath = parts[3].Replace('\\', Path.DirectorySeparatorChar);
                var candidatePath = Path.GetFullPath(
                    Path.Combine(Path.GetDirectoryName(solutionPath)!, solutionProjectPath)
                );
                if (PathsEqual(candidatePath, fullProjectPath))
                {
                    return true;
                }
            }
        }
        catch (Exception)
        {
            // Let dotnet sln report malformed solution files when the direct check cannot parse them.
        }

        return false;
    }

    private static bool PathsEqual(string left, string right)
    {
        return string.Equals(
            Path.GetFullPath(left).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            Path.GetFullPath(right).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar),
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static string QuoteCommandArgument(string value)
    {
        return $"\"{value.Replace("\"", "\\\"")}\"";
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
            var targetFramework = existService is null
                ? ConstVal.NetVersion
                : AssemblyHelper.GetTargetFramework(existService.Path) ?? ConstVal.NetVersion;
            var csprojContent = TplContent.ServiceProjectFileTpl(targetFramework);
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
                $"build {QuoteCommandArgument(projectPath)} {(restore ? "" : "--no-restore")}",
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
        if (!File.Exists(projectPath))
        {
            OutputHelper.Error($"Project file not found: {projectPath}");
            return false;
        }

        if (!File.Exists(referencePath))
        {
            OutputHelper.Error($"Referenced project file not found: {referencePath}");
            return false;
        }

        if (await HasProjectReferenceAsync(projectPath, referencePath))
        {
            return true;
        }
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"add {QuoteCommandArgument(projectPath)} reference {QuoteCommandArgument(referencePath)}",
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
        var projectContent = await File.ReadAllTextAsync(projectPath);
        try
        {
            var document = XDocument.Parse(projectContent);
            var projectDirectory = Path.GetDirectoryName(Path.GetFullPath(projectPath))!;
            var requestedName = Path.GetFileNameWithoutExtension(referenceName.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar
            ));
            var requestedPath = Path.IsPathRooted(referenceName)
                || referenceName.Contains(Path.DirectorySeparatorChar)
                || referenceName.Contains(Path.AltDirectorySeparatorChar)
                ? Path.GetFullPath(referenceName)
                : null;

            foreach (var element in document.Descendants().Where(
                         item => item.Name.LocalName.Equals("ProjectReference", StringComparison.OrdinalIgnoreCase)))
            {
                var include = element.Attribute("Include")?.Value;
                if (string.IsNullOrWhiteSpace(include))
                {
                    continue;
                }

                include = include.Replace('\\', Path.DirectorySeparatorChar);

                if (requestedPath is not null)
                {
                    var includedPath = Path.GetFullPath(Path.Combine(projectDirectory, include));
                    if (PathsEqual(includedPath, requestedPath))
                    {
                        return true;
                    }
                }

                if (string.Equals(
                        Path.GetFileNameWithoutExtension(include),
                        requestedName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        catch (XmlException)
        {
            var normalizedReference = referenceName.Replace('\\', '/');
            return projectContent.Contains(normalizedReference, StringComparison.OrdinalIgnoreCase)
                || projectContent.Contains(Path.GetFileName(referenceName), StringComparison.OrdinalIgnoreCase);
        }
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
