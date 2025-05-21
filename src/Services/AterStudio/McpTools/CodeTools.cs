using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools(ILogger<CodeTools> logger)
{
    [McpServerTool, Description("创建实体模型类")]
    public string? NewEntity([Description("用户输入的提示词内容")] string prompt)
    {
        var message = Prompts.CreateEntity();

        var res = $"""
            <prompt>
            {prompt}
            </prompt>

            {message.ToString()}
            """;

        //logger.LogInformation(res);
        return res;
    }

    [McpServerTool, Description("根据实体模型生成Dto")]
    public string? GenerateDto([Description("实体模型文件的绝对路径")] string entityPath)
    {
        logger.LogInformation(entityPath);
        return "DTO Generation Completed";
    }

    [McpServerTool, Description("根据实体模型生成Manager")]
    public string? GenerateManager([Description("实体模型文件的绝对路径")] string entityPath)
    {
        logger.LogInformation(entityPath);
        return "Manager Generation Completed";
    }

    [McpServerTool, Description("根据实体模型生成Controller")]
    public string? GenerateController([Description("实体模型文件的绝对路径")] string entityPath)
    {
        logger.LogInformation(entityPath);
        return "Controller Generation Completed";
    }

    [McpServerTool, Description("生成前端请求服务")]
    public string? GenerateService([Description("openapi的url地址或本地路径")] string openApiPath, [Description("生成的目标根目录")] string outputPath, [Description("前端请求类型,NgHttp或Axios")] RequestLibType clientType)
    {
        logger.LogInformation(openApiPath, outputPath, clientType.ToString());
        return "Service Generation Completed";
    }
}
