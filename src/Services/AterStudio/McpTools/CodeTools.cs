using System.ComponentModel;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools(ILogger<CodeTools> logger, EntityInfoManager manager)
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
    public async Task<string?> GenerateDtoAsync([Description("实体模型文件的绝对路径")] string entityPath)
    {
        return await GenerateAsync(entityPath, CommandType.Dto);
    }

    [McpServerTool, Description("根据实体模型生成Manager")]
    public async Task<string?> GenerateManagerAsync([Description("实体模型文件的绝对路径")] string entityPath)
    {
        return await GenerateAsync(entityPath, CommandType.Manager);
    }

    [McpServerTool, Description("根据实体模型生成Controller")]
    public async Task<string?> GenerateControllerAsync([Description("实体模型文件的绝对路径")] string entityPath)
    {
        return await GenerateAsync(entityPath, CommandType.API);
    }

    [McpServerTool, Description("生成前端请求服务")]
    public string? GenerateService([Description("openapi的url地址或本地路径")] string openApiPath, [Description("生成的目标根目录")] string outputPath, [Description("前端请求类型,NgHttp或Axios")] RequestLibType clientType)
    {
        logger.LogInformation(openApiPath, outputPath, clientType.ToString());
        return "Service Generation Completed";
    }

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <returns></returns>
    private async Task<string> GenerateAsync(string entityPath, CommandType type)
    {

        logger.LogInformation($"生成{type}，路径：{entityPath}");
        var dto = new GenerateDto
        {
            EntityPath = entityPath,
            CommandType = type,
            Force = true,
        };

        var res = await manager.GenerateAsync(dto);
        var resDes = string.Empty;
        foreach (var file in res)
        {
            resDes += $"已生成文件{file.Name}，路径: {file.FullName}.{Environment.NewLine}";
        }
        return resDes;
    }

}
