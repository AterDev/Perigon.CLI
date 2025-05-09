using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using CodeGenerator.Helper;
using Entity.StudioMod;
using Microsoft.Extensions.Logging;
using Share.Infrastructure.Helper;

namespace Command.Share.Commands;
public class StudioCommand(ILogger<StudioCommand> logger)
{
    private readonly ILogger<StudioCommand> _logger = logger;

    public async Task RunStudioAsync()
    {
        _logger.LogInformation("🙌 Welcome Ater studio!");
        string studioPath = AssemblyHelper.GetStudioPath();
        int sleepTime = 1500;
        // 检查并更新
        string version = AssemblyHelper.GetCurrentToolVersion();
        if (File.Exists(Path.Combine(studioPath, $"{version}.txt")))
        {
            _logger.LogInformation("😊 Already latest version!");
        }
        else
        {
            UpdateStudio();
        }
        _logger.LogInformation("🚀 start studio...");
        // 运行
        string shell = "dotnet";
        var port = GetAvailablePort();
        _logger.LogInformation("可用端口:{port}", port);
        string url = $"http://localhost:{port}";

        var args = Path.Combine(studioPath, ConstVal.StudioFileName) + $" --urls \"{url}\"";

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
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}")
                    {
                        CreateNoWindow = true
                    });
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
                    _logger.LogInformation("start browser failed: {message}", ex.Message);
                }
            }
            await process.WaitForExitAsync();
        }
    }
    /// <summary>
    /// 升级studio
    /// </summary>
    public void UpdateStudio()
    {
        _logger.LogInformation($"☑️ check&update studio...");
        string[] copyFiles =
        [
            "Ater.Web.Core",
            "CodeGenerator",
            "Entity",
            "Humanizer",
            "Mapster.Core",
            "Mapster",
            "Microsoft.AspNetCore.Razor.Language",
            "Microsoft.Build",
            "Microsoft.Build.Framework",
            "Microsoft.Build.Locator",
            "Microsoft.Build.Tasks.Core",
            "Microsoft.Build.Utilities.Core",
            "Microsoft.CodeAnalysis.CSharp",
            "Microsoft.CodeAnalysis.CSharp.Workspaces",
            "Microsoft.CodeAnalysis",
            "Microsoft.CodeAnalysis.ExternalAccess.RazorCompiler",
            "Microsoft.CodeAnalysis.Workspaces",
            "Microsoft.CodeAnalysis.Workspaces.MSBuild",
            "Microsoft.EntityFrameworkCore.Abstractions",
            "Microsoft.Extensions.Configuration.Abstractions",
            "Microsoft.Extensions.DependencyInjection.Abstractions",
            "Microsoft.Extensions.DependencyInjection",
            "Microsoft.Extensions.Logging.Abstractions",
            "Microsoft.Extensions.Logging",
            "Microsoft.Extensions.Options",
            "Microsoft.Extensions.Primitives",
            "Microsoft.NET.StringTools",
            "Microsoft.OpenApi",
            "Microsoft.OpenApi.Readers",
            "Microsoft.VisualStudio.Setup.Configuration.Interop",
            "Newtonsoft.Json",
            "RazorEngineCore",
            "Share",
            "SharpYaml",
            "System.CodeDom",
            "System.Composition.AttributedModel",
            "System.Composition.Convention",
            "System.Composition.Hosting",
            "System.Composition.Runtime",
            "System.Composition.TypedParts",
            "System.Configuration.ConfigurationManager",
            "System.Diagnostics.DiagnosticSource",
            "System.IO.Pipelines",
            "System.Reflection.MetadataLoadContext",
            "System.Resources.Extensions",
            "System.Security.Cryptography.ProtectedData",
            "System.Security.Permissions",
            "System.Text.Encodings.Web",
            "System.Text.Json",
            "System.Windows.Extensions",
        ];

        string version = AssemblyHelper.GetCurrentToolVersion();
        string toolRootPath = AssemblyHelper.GetToolPath();
        string zipPath = Path.Combine(toolRootPath, ConstVal.StudioZip);
        string templatePath = Path.Combine(toolRootPath, ConstVal.TemplateZip);

        if (!File.Exists(zipPath))
        {
            _logger.LogInformation($"not found studio.zip in:{toolRootPath}");
            return;
        }
        string studioPath = AssemblyHelper.GetStudioPath();
        string dbFile = Path.Combine(studioPath, ConstVal.DbName);
        string tempDbFile = Path.Combine(Path.GetTempPath(), ConstVal.DbName);

        // 删除旧文件
        if (Directory.Exists(studioPath))
        {
            if (File.Exists(dbFile))
            {
                File.Copy(dbFile, tempDbFile, true);
            }
            _logger.LogInformation("delete {path}", studioPath);
            Directory.Delete(studioPath, true);
        }

        // 解压
        if (File.Exists(templatePath))
        {
            ZipFile.ExtractToDirectory(templatePath, studioPath, true);
        }
        ZipFile.ExtractToDirectory(zipPath, studioPath, true);
        _logger.LogInformation("extract {zip} to {path}", zipPath, studioPath);

        if (File.Exists(tempDbFile))
        {
            File.Copy(tempDbFile, dbFile, true);
        }

        // copy其他文件
        _logger.LogInformation("start copy {0} files to {studioPath}", copyFiles.Count(), studioPath);
        copyFiles.ToList().ForEach(file =>
        {
            string sourceFile = Path.Combine(toolRootPath, file + ".dll");
            if (File.Exists(sourceFile))
            {
                File.Copy(sourceFile, Path.Combine(studioPath, file + ".dll"), true);
            }
            else
            {
                _logger.LogWarning("{source} file not exist!", sourceFile);
            }
        });

        string runtimesDir = Path.Combine(toolRootPath, "BuildHost-netcore");
        if (Directory.Exists(runtimesDir))
        {
            string targetDir = Path.Combine(studioPath, "BuildHost-netcore");
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

        // create version file
        File.Create(Path.Combine(studioPath, $"{version}.txt")).Close();

        UpdateTemplate();
        _logger.LogInformation("✅ update complete!");
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
            if (endPoint.Port == defaultPort) return alternative;
        }

        var endPointsUdp = properties.GetActiveUdpListeners();
        foreach (var endPoint in endPointsUdp)
        {
            if (endPoint.Port == defaultPort) return alternative;
        }
        return defaultPort;
    }
    /// <summary>
    /// 下载或更新模板
    /// </summary>
    public void UpdateTemplate()
    {
        // 安装模板
        if (!ProcessHelper.RunCommand("dotnet", "new list atapi", out string _))
        {
            if (!ProcessHelper.RunCommand("dotnet", "new install ater.web.templates", out _))
            {
                _logger.LogInformation("⚠️ ater.web.templates install failed!");
            }
        }
        else
        {
            if (ProcessHelper.RunCommand("dotnet", "new update", out string _))
            {
            }
        }
    }
}
