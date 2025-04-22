using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Framework.Common.Utils;
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
    public static bool RunCommand(string command, string? args, out string output)
    {
        var process = new Process();
        process.StartInfo.FileName = command;
        process.StartInfo.Arguments = args;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;

        var outputBuilder = new StringBuilder();
        var outputErrorBuilder = new StringBuilder();

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
                outputErrorBuilder.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var errorOutput = outputErrorBuilder.ToString();
        output = outputBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    /// <summary>
    /// 执行命令，使用cmd/bash
    /// </summary>
    /// <param name="commands"></param>
    /// <returns></returns>
    public static string ExecuteCommands(params string[] commands)
    {
        string shell, argument;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            shell = "powershell";
            argument = "/c";
        }
        else
        {
            shell = "/bin/bash";
            argument = "-c";
        }

        var commandString = string.Join(" && ", commands);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = shell,
                Arguments = $"{argument} \"{commandString}\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var outputBuilder = new StringBuilder();
        var outputErrorBuilder = new StringBuilder();

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
                outputErrorBuilder.AppendLine(e.Data);
            }
        };

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        var errorOutput = outputErrorBuilder.ToString();
        var output = outputBuilder.ToString();
        if (!string.IsNullOrWhiteSpace(errorOutput))
        {
            return errorOutput;
        }
        else
        {
            return output;
        }
    }


    /// <summary>
    /// 执行命令
    /// </summary>
    /// <param name="fileName"></param>
    /// <param name="commands"></param>
    /// <param name="workingDirectory"></param>
    /// <param name="environmentVariables"></param>
    /// <returns></returns>
    public static string ExecuteCommands(string fileName, string[] commands, string? workingDirectory = null, StringDictionary? environmentVariables = null)
    {
        var commandString = string.Join(" && ", commands);
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = commandString,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
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
                    process.StartInfo.EnvironmentVariables.Add(entry.Key.ToString()!, entry.Value!.ToString());
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
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();
        return outputBuilder.ToString();
    }
}
