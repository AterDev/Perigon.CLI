using Microsoft.EntityFrameworkCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AterStudio.McpTools;

public class McpToolsHandler(IDbContextFactory<DefaultDbContext> dbContext)
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

    public ValueTask<CallToolResult> CallToolHandler(
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
                    var promptContent = File.ReadAllText(tool.PromptPath);
                    var templateContents = tool.TemplatePaths
                        .Select(path => File.ReadAllText(path));

                    var prompt = $"""
                    {promptContent}，提供良好的代码格式和规范。
                    <example>
                    {string.Join(Environment.NewLine, templateContents)}
                    </example>
                    """;
                    return prompt;
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
