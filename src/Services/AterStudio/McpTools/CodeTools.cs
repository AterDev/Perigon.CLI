using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools
{
    [McpServerTool, Description("Provides help with code generation.")]
    public string Help()
    {
        return "This tool helps with project code generation.";
    }


    [McpServerTool, Description("Echoes in reverse the message sent by the client.")]
    public static string ReverseEcho(string message) => new string(message.Reverse().ToArray());
}
