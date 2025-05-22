using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

/// <summary>
/// 代码生成MCP工具
/// </summary>
[McpServerToolType]
public class CodeTools(
    ILogger<CodeTools> logger,
    EntityInfoManager manager,
    IProjectContext projectContext)
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
    public async Task<string?> GenerateDtoAsync(IMcpServer server, [Description("实体模型文件的绝对路径")] string entityPath)
    {
        return await GenerateAsync(server, entityPath, CommandType.Dto);
    }

    [McpServerTool, Description("根据实体模型生成Manager")]
    public async Task<string?> GenerateManagerAsync([Description("实体模型文件的绝对路径")] string entityPath, IMcpServer server)
    {
        return await GenerateAsync(server, entityPath, CommandType.Manager);
    }

    [McpServerTool, Description("根据实体模型生成Controller")]
    public async Task<string?> GenerateControllerAsync([Description("实体模型文件的绝对路径")] string entityPath, IMcpServer server)
    {
        return await GenerateAsync(server, entityPath, CommandType.API);
    }

    [McpServerTool, Description("生成前端请求服务")]
    public string? GenerateService(
        [Description("openapi的url地址或本地路径")] string openApiPath,
        [Description("生成的目标根目录")] string outputPath,
        [Description("前端请求类型,NgHttp或Axios")] RequestLibType clientType)
    {
        logger.LogInformation(openApiPath, outputPath, clientType.ToString());
        return "Service Generation Completed";
    }

    /// <summary>
    /// 生成服务
    /// </summary>
    /// <returns></returns>
    private async Task<string> GenerateAsync(IMcpServer server, string entityPath, CommandType type)
    {
        var roots = await server.RequestRootsAsync(new ModelContextProtocol.Protocol.ListRootsRequestParams
        {
            Meta = new ModelContextProtocol.Protocol.RequestParamsMetadata
            {
                ProgressToken = new ModelContextProtocol.Protocol.ProgressToken("CodeTools")
            }
        });

        try
        {
            if (roots.Roots.Count > 0)
            {
                foreach (var root in roots.Roots)
                {
                    logger.LogInformation($"获取到的根目录：{root.Name}, {root.Uri}, {root?.Meta?.ToString()}");
                }
            }

            var uri = roots.Roots.FirstOrDefault()?.Uri;
            if (string.IsNullOrEmpty(uri))
            {
                logger.LogError("未找到有效的根目录路径。");
                return "Not get client roots";
            }

            var solutionPath = new Uri(uri).LocalPath;
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                if (solutionPath.StartsWith('/'))
                {
                    solutionPath = solutionPath[1..];
                }
            }

            await projectContext.SetProjectAsync(solutionPath);
            var dto = new GenerateDto
            {
                EntityPath = entityPath,
                CommandType = type,
                Force = true,
            };

            var res = await manager.GenerateAsync(dto);
            var resDes = new StringBuilder("<result>");
            foreach (var file in res)
            {
                resDes.Append("成功生成文件:")
                      .Append(file.Name)
                      .Append("，路径: ")
                      .AppendLine(file.FullName);

            }
            resDes.Append("</result>");
            return resDes.ToString();
        }
        catch (Exception ex)
        {
            logger.LogError("{ex}", ex);
            return "工具出错：" + ex.Message;
        }

    }

}
