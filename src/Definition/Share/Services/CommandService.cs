using CodeGenerator.Helper;
using Entity;
using Share.Models.CommandDtos;
using Spectre.Console;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.Json.Nodes;

namespace Share.Services;

/// <summary>
///  command service
/// </summary>
/// <param name="context"></param>
/// <param name="projectContext"></param>
/// <param name="solutionService"></param>
/// <param name="codeGenService"></param>
public class CommandService(
    DefaultDbContext context,
    IProjectContext projectContext,
    SolutionService solutionService,
    CodeGenService codeGenService
)
{
    public string? ErrorMsg { get; set; }

    public async Task<int?> AddProjectAsync(string name, string path)
    {
        var projectFilePath = Directory
            .GetFiles(path, $"*{ConstVal.SolutionExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        projectFilePath ??= Directory
            .GetFiles(path, $"*{ConstVal.SolutionXMLExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        projectFilePath ??= Directory
            .GetFiles(path, $"*{ConstVal.CSharpProjectExtension}", SearchOption.TopDirectoryOnly)
            .FirstOrDefault();
        projectFilePath ??= Directory
            .GetFiles(path, ConstVal.NodeProjectFile, SearchOption.TopDirectoryOnly)
            .FirstOrDefault();

        var solutionType = AssemblyHelper.GetSolutionType(projectFilePath);
        var solutionName = Path.GetFileName(projectFilePath) ?? name;
        var solutionPath = Path.GetDirectoryName(projectFilePath) ?? "";
        var solution = new Solution()
        {
            DisplayName = name,
            Path = solutionPath,
            Name = solutionName,
            SolutionType = solutionType,
        };
        solution.Config.SolutionPath = solutionPath;

        context.Solutions.Add(solution);
        await context.SaveChangesAsync();
        return solution.Id;
    }

    public async Task<bool> CreateSolutionAsync(CreateSolutionDto dto)
    {
        // 生成项目
        string solutionPath = Path.Combine(dto.Path, dto.Name);
        string templateType = dto.IsLight ? ConstVal.Mini : ConstVal.WebApi;

        string version = AssemblyHelper.GetCurrentToolVersion();

        if (ProcessHelper.RunCommand("dotnet", $"new list {templateType}", out _))
        {
            ProcessHelper.RunCommand("dotnet", $"new update", out _);
        }
        else
        {
            ProcessHelper.RunCommand(
                "dotnet",
                $"new install {ConstVal.TemplatePackageId}::{ConstVal.TemplateVersion}",
                out string msg
            );
            OutputHelper.Info(msg);
        }

        if (!Directory.Exists(dto.Path))
        {
            Directory.CreateDirectory(solutionPath);
        }

        var templateOptions = string.Empty;
        if (dto.FrontType != FrontType.None)
        {
            templateOptions = $" --frontType {dto.FrontType}";
        }
        if (
            !ProcessHelper.RunCommand(
                "dotnet",
                $"new {templateType} -o {solutionPath} --force {templateOptions}",
                out string error
            )
        )
        {
            OutputHelper.Error(error);
            ErrorMsg = "Create failed, check the error output.";
            return false;
        }
        OutputHelper.Success($"Created new solution {solutionPath}");

        try
        {
            var id = await solutionService.SaveSolutionAsync(solutionPath, dto.Name);
            await projectContext.SetProjectByIdAsync(id);
        }
        catch (Exception ex)
        {
            OutputHelper.Error($"Failed to save solution: {ex.Message}");
            ErrorMsg = $"Failed to save solution: {ex.Message}";
            return false;
        }

        OutputHelper.Important($"Apply settings...");

        // 更新服务的配置文件
        var services = solutionService.GetServices();
        if (services != null)
        {
            foreach (var service in services)
            {
                var servicePath = Path.Combine(solutionPath, PathConst.ServicesPath, service.Name);
                UpdateAppSettings(dto, servicePath);
            }
        }
        // 更新Aspire Host配置文件
        var aspirePath = Path.Combine(solutionPath, PathConst.AppHostPath);
        UpdateAppSettings(dto, aspirePath);

        // 前端项目处理
        if (dto.FrontType == FrontType.None)
        {
            string appPath = Path.Combine(
                solutionPath,
                "src",
                "ClientApp",
                "WebApp"
            );
            if (Directory.Exists(appPath))
            {
                Directory.Delete(appPath, true);
            }
        }
        // 添加模块到解决方案中
        if (!dto.IsLight && dto.Modules.Count > 0)
        {
            SolutionService.AddDefaultModule(ModuleInfo.User, solutionPath);
            foreach (string item in dto.Modules)
            {
                OutputHelper.Important($"Add module:{item}");
                SolutionService.AddDefaultModule(item, solutionPath);
            }
        }

        SolutionService.BuildSourceGeneration(solutionPath);
        if (projectContext.EntityPath.NotEmpty())
        {
            SolutionService.BuildProject(projectContext.EntityPath);
        }
        if (projectContext.EntityFrameworkPath.NotEmpty())
        {
            SolutionService.BuildProject(projectContext.EntityFrameworkPath);
        }
        // restore dotnet tools
        if (!ProcessHelper.RunCommand("dotnet", "tool restore", out string restoreMsg))
        {
            OutputHelper.Error(restoreMsg);
        }

        OutputHelper.Success($"Create solution {dto.Name} completed!");
        return true;
    }


    public async Task GenerateRequestClientAsync(
        string url,
        string outputPath,
        RequestClientType type,
        bool onlyModels = false
    )
    {
        try
        {
            List<GenFileInfo> genFiles = [];
            if (type == RequestClientType.CSharp)
            {
                genFiles = await codeGenService.GenerateCsharpApiClientAsync(url, outputPath, onlyModels);
            }
            else
            {
                genFiles = await codeGenService.GenerateWebRequestAsync(
                   url,
                   outputPath,
                   type,
                   onlyModels
               ) ?? [];
            }
            codeGenService.GenerateFiles(genFiles);
        }
        catch (Exception ex)
        {
            OutputHelper.Error(ex.Message + ex.StackTrace);
        }
    }

    /// <summary>
    /// 更新配置文件
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="path"></param>
    private static void UpdateAppSettings(CreateSolutionDto dto, string path)
    {
        // 修改配置文件
        string configFile = Path.Combine(path, ConstVal.AppSettingDevelopmentJson);
        if (!File.Exists(configFile))
            return;
        string jsonString = File.ReadAllText(configFile);
        JsonNode? jsonNode = JsonNode.Parse(
            jsonString,
            documentOptions: new JsonDocumentOptions { CommentHandling = JsonCommentHandling.Skip }
        );
        if (jsonNode != null)
        {
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Database", dto.DBType.ToString());
            JsonHelper.AddOrUpdateJsonNode(jsonNode, "Components.Cache", dto.CacheType.ToString());

            if (dto.CommandDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.CommandDb",
                    dto.CommandDbConnStrings
                );
            }

            if (dto.QueryDbConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.QueryDb",
                    dto.QueryDbConnStrings
                );
            }
            if (dto.CacheConnStrings.NotEmpty())
            {
                JsonHelper.AddOrUpdateJsonNode(
                    jsonNode,
                    "ConnectionStrings.Cache",
                    dto.CacheConnStrings
                );
            }
            JsonHelper.AddOrUpdateJsonNode(
                jsonNode,
                "ConnectionStrings.CacheInstanceName",
                dto.CacheInstanceName ?? "Dev"
            );

            jsonString = jsonNode.ToString();
            File.WriteAllText(configFile, jsonString);
        }
    }

    public static async Task RunStudioAsync()
    {
        var cpuThread = Environment.ProcessorCount;
        int sleepTime = 3000 - (100 * cpuThread);
        sleepTime = sleepTime < 1000 ? 1000 : sleepTime;

        string studioPath = AssemblyHelper.GetStudioPath();
        // 检查并更新
        string version = AssemblyHelper.GetCurrentToolVersion();
        OutputHelper.Info($"current version:{version}");
        if (!File.Exists(Path.Combine(studioPath, $"{version}.txt")))
        {
            OutputHelper.Important($"find new version：{version}");
            UpdateStudio();
        }

        OutputHelper.Info("🚀 start studio...");
        string shell = "dotnet";
        var port = GetAvailablePort();
        OutputHelper.Info($"using port:{port}");
        string url = $"http://localhost:{port}";

        Process process = new()
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"./{ConstVal.StudioFileName} --urls \"{url}\"",
                UseShellExecute = false,
                CreateNoWindow = false,
                //RedirectStandardOutput = true,
                WorkingDirectory = studioPath,
            },
        };

        if (process.Start())
        {
            await Task.Delay(sleepTime);
            try
            {
                Process.Start(url).Close();
            }
            catch (Exception ex)
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(
                        new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true }
                    );
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    OutputHelper.Error($"can't start studio: {ex.Message}");
                }
            }
            await process.WaitForExitAsync();
        }
    }

    /// <summary>
    /// 升级studio
    /// </summary>
    public static void UpdateStudio()
    {
        string[] copyFiles = [];

        string version = AssemblyHelper.GetCurrentToolVersion();
        string toolRootPath = AssemblyHelper.GetToolPath();
        string zipPath = Path.Combine(toolRootPath, ConstVal.StudioZip);
        string modulesPath = Path.Combine(toolRootPath, ConstVal.ModulesZip);
        string shareDllsPath = Path.Combine(toolRootPath, ConstVal.ShareDlls);
        if (File.Exists(shareDllsPath))
        {
            copyFiles = File.ReadAllLines(shareDllsPath);
        }
        else
        {
            OutputHelper.Warning($"not found {ConstVal.ShareDlls} in:{toolRootPath}");
        }
        if (!File.Exists(zipPath))
        {
            OutputHelper.Error($"not found studio.zip in:{toolRootPath}");
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string dbFile = Path.Combine(studioPath, ConstVal.DbName);
        string tempDbFile = Path.Combine(Path.GetTempPath(), ConstVal.DbName);

        AnsiConsole
            .Status()
            .Spinner(Spinner.Known.Aesthetic)
            .Start(
                "Updating...",
                ctx =>
                {
                    // 删除旧文件
                    if (Directory.Exists(studioPath))
                    {
                        ctx.Status("delete old files");
                        if (File.Exists(dbFile))
                        {
                            File.Copy(dbFile, tempDbFile, true);
                        }
                        Directory.Delete(studioPath, true);
                        OutputHelper.Important($"delete {studioPath}");
                    }

                    // 解压
                    ctx.Status("copy new files");
                    if (File.Exists(modulesPath))
                    {
                        ZipFile.ExtractToDirectory(modulesPath, studioPath, true);
                    }
                    ZipFile.ExtractToDirectory(zipPath, studioPath, true);
                    OutputHelper.Important($"extract {zipPath} to {studioPath}");

                    if (File.Exists(tempDbFile))
                    {
                        OutputHelper.Info($"recover db file");
                        File.Copy(tempDbFile, dbFile, true);
                    }

                    // copy其他文件
                    OutputHelper.Important($"start copy {copyFiles.Length} files to {studioPath}");
                    copyFiles
                        .ToList()
                        .ForEach(file =>
                        {
                            string sourceFile = Path.Combine(toolRootPath, file);
                            if (File.Exists(sourceFile))
                            {
                                File.Copy(sourceFile, Path.Combine(studioPath, file), true);
                            }
                            else
                            {
                                OutputHelper.Info($"{sourceFile} file not exist!");
                            }
                        });

                    string runtimesDir = Path.Combine(toolRootPath, "BuildHost-netcore");
                    string targetDir = Path.Combine(studioPath, "BuildHost-netcore");
                    CreateSymbolicLink(targetDir, runtimesDir);
                    runtimesDir = Path.Combine(toolRootPath, "runtimes");
                    targetDir = Path.Combine(studioPath, "runtimes");
                    CreateSymbolicLink(targetDir, runtimesDir);

                    // create version file
                    File.Create(Path.Combine(studioPath, $"{version}.txt")).Close();
                    ctx.Status("update template");
                    UpdateTemplate();
                    OutputHelper.Success($"update to {version} completed!");
                    ctx.Status("Done!");
                    ctx.Refresh();
                }
            );
    }

    /// <summary>
    /// 创建软链接
    /// </summary>
    /// <param name="targetDir"></param>
    /// <param name="runtimesDir"></param>
    private static void CreateSymbolicLink(string targetDir, string runtimesDir)
    {
        if (Directory.Exists(runtimesDir))
        {
            if (Directory.Exists(targetDir))
            {
                Directory.Delete(targetDir, true);
            }
            try
            {
                Directory.CreateSymbolicLink(targetDir, runtimesDir);
            }
            catch (Exception)
            {
                IOHelper.CopyDirectory(runtimesDir, targetDir);
            }
        }
    }

    /// <summary>
    /// 获取可用端口
    /// </summary>
    /// <returns></returns>
    public static int GetAvailablePort(int alternative = 9160)
    {
        var defaultPort = 19160;
        var properties = IPGlobalProperties.GetIPGlobalProperties();

        var endPointsTcp = properties.GetActiveTcpListeners();
        foreach (var endPoint in endPointsTcp)
        {
            if (endPoint.Port == defaultPort)
                return alternative;
        }

        var endPointsUdp = properties.GetActiveUdpListeners();
        foreach (var endPoint in endPointsUdp)
        {
            if (endPoint.Port == defaultPort)
                return alternative;
        }
        return defaultPort;
    }

    /// <summary>
    /// 下载或更新模板
    /// </summary>
    public static void UpdateTemplate()
    {
        // 安装模板
        if (!ProcessHelper.RunCommand("dotnet", "new list ater", out string _))
        {
            if (!ProcessHelper.RunCommand("dotnet", "new install Perigon.templates", out _))
            {
                OutputHelper.Warning("Perigon.templates install failed!");
            }
        }
        else
        {
            if (ProcessHelper.RunCommand("dotnet", "new update", out string _)) { }
        }
    }
}
