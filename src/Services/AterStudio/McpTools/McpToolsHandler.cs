using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

public class McpToolsHandler(
    IDbContextFactory<DefaultDbContext> dbContext,
    ILogger<McpToolsHandler> logger
)
{
    public async ValueTask<ListToolsResult> ListToolsHandler(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        var result = new ListToolsResult();
        var defaultTools = request.Server.ServerOptions.ToolCollection ?? [];
        using var context = dbContext.CreateDbContext();
        var tools = await context.McpTools.ToListAsync();

        foreach (var tool in tools)
        {
            AddMcpTool(defaultTools, tool);
        }
        foreach (var tool in defaultTools)
        {
            result.Tools.Add(
                new Tool
                {
                    Name = tool.ProtocolTool.Name,
                    Description = tool.ProtocolTool.Description,
                    Title = tool.ProtocolTool.Title,
                }
            );
        }
        return result;
    }

    public ValueTask<CallToolResult> CallToolsHandler(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        var result = new CallToolResult();
        // 原始工具名
        var name = request.Params?.Name;
        var args = request.Params?.Arguments;
        // 这里记录原始参数
        Console.WriteLine($"ToolInvoke: {name}");
        return ValueTask.FromResult(result);
    }

    private static void AddMcpTool(
        McpServerPrimitiveCollection<McpServerTool> collection,
        McpTool tool
    )
    {
        if (collection.TryGetPrimitive(tool.Name, out var existingTool))
        {
            return;
        }
        collection.Add(
            McpServerTool.Create(
                () =>
                {
                    var prompt = $"""
                    根据以下 prompt和 template 内容生成代码，下面将提供prompt和tempalte的本地路径：
                        <prompt>
                        {tool.PromptPath}
                        </prompt>
                        <template>
                        {string.Join(Environment.NewLine, tool.TemplatePaths)}
                        </template>
                    """;
                },
                new McpServerToolCreateOptions
                {
                    Name = tool.Name,
                    Description = tool.Description,
                    Title = tool.Description,
                }
            )
        );
    }

    private static void DeleteMcpTool(
        McpServerPrimitiveCollection<McpServerTool> collection,
        string toolName
    )
    {
        if (collection.TryGetPrimitive(toolName, out var tool))
        {
            collection.Remove(tool);
        }
    }
}
