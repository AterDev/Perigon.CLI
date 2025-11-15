using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Ater.AspNetCore.Utils;

/// <summary>
/// 调用帮助类
/// </summary>
public static class ProcessHelper
{
    /// <summary>
    /// 运行命令
    /// </summary>
    /// <param name="command">命令程序</param>
    /// <param name="args">参数</param>
    /// <param name="output"></param>
    /// <returns></returns>
    public static bool RunCommand(
        string command,
        string? args,
        out string output,
        int timeoutMilliseconds = 30000
    )
    {
        output = string.Empty;
        var error = string.Empty;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = command,
                    Arguments = args ?? string.Empty,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true,
                },
            };

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                }
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(timeoutMilliseconds))
            {
                try
                {
                    process.Kill();
                }
                catch { }
                error = "命令执行超时";
                return false;
            }

            output = outputBuilder.ToString();
            error = errorBuilder.ToString();
            if (string.IsNullOrWhiteSpace(error))
            {
                return true;
            }
            else
            {
                output += Environment.NewLine + "Error: " + error;
                return false;
            }
        }
        catch (Exception ex)
        {
            error = $"Run command errors: {ex.Message}";
            return false;
        }
    }

    /// <summary>
    /// 执行命令，使用cmd/bash
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public static string ExecuteCommands(params string[] commands)
    {
        if (commands == null || commands.Length == 0)
        {
            return string.Empty;
        }

        string shell;
        string argument;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell = "powershell";
            argument = "-Command";
        }
        else
        {
            shell = "/bin/bash";
            argument = "-c";
        }

        var commandString = string.Join(" && ", commands);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{argument} \"{commandString}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = false,
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (_, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var errorOutput = errorBuilder.ToString();
        return !string.IsNullOrWhiteSpace(errorOutput) ? errorOutput : outputBuilder.ToString();
    }

    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="commands"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariables"></param>
    /// <returns></returns>
    public static string ExecuteCommands(
        string fileName,
        string[] commands,
        string? workingDirectory = null,
        StringDictionary? environmentVariables = null
    )
    {
        var commandString = string.Join(" && ", commands);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = commandString,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            },
            EnableRaisingEvents = false,
        };

        if (workingDirectory is not null)
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }
        if (environmentVariables is not null)
        {
            foreach (DictionaryEntry entry in environmentVariables)
            {
                if (entry.Key != null)
                {
                    process.StartInfo.EnvironmentVariables.Add(
                        entry.Key.ToString()!,
                        entry.Value!.ToString()
                    );
                }
            }
        }
        var outputBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        return outputBuilder.ToString();
    }
}
