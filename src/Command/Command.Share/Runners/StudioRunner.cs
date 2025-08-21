using System.Diagnostics;
using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using CodeGenerator.Helper;
using Entity;
using Share.Helper;

namespace Command.Share.Runners;

public class StudioRunner
{
    public static async Task RunStudioAsync()
    {
        var cpuThread = Environment.ProcessorCount;
        int sleepTime = 3000 - 100 * cpuThread;
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
            if (!ProcessHelper.RunCommand("dotnet", "new install ater.web.templates", out _))
            {
                OutputHelper.Warning("ater.web.templates install failed!");
            }
        }
        else
        {
            if (ProcessHelper.RunCommand("dotnet", "new update", out string _)) { }
        }
    }
}
