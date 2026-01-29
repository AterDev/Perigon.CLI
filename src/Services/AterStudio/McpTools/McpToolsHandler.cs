using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Reflection;

namespace AterStudio.McpTools;

public class McpToolsHandler(DefaultDbContext dbContext, ILogger<McpToolsHandler> logger)
{
    private List<McpServerTool>? _cachedCodeTools;

    public async ValueTask<ListToolsResult> ListToolsHandler(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        var result = new ListToolsResult();

        // 从 CodeTools 获取所有工具
        if (_cachedCodeTools == null)
        {
            _cachedCodeTools = BuildCodeToolsList();
        }

        logger.LogInformation("[ListTools] CodeTools count: {Count}", _cachedCodeTools.Count);

        foreach (var tool in _cachedCodeTools)
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

        // 添加数据库中的工具
        var dbTools = dbContext.McpTools.ToList();
        logger.LogInformation("[ListTools] DB tools count: {Count}", dbTools.Count);

        foreach (var dbTool in dbTools)
        {
            // 避免重复
            if (!result.Tools.Any(t => t.Name == dbTool.Name))
            {
                result.Tools.Add(
                    new Tool
                    {
                        Name = dbTool.Name,
                        Description = dbTool.Description,
                        Title = dbTool.Description,
                    }
                );
            }
        }

        logger.LogInformation("[ListTools] Total tools returned: {Count}", result.Tools.Count);
        return result;
    }

    private List<McpServerTool> BuildCodeToolsList()
    {
        var tools = new List<McpServerTool>();
        var codeToolsType = typeof(CodeTools);

        // 获取所有标记了 [McpServerTool] 的方法
        var methods = codeToolsType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<McpServerToolAttribute>() != null)
            .ToList();

        logger.LogInformation("[BuildTools] Found {Count} methods in CodeTools", methods.Count);

        foreach (var method in methods)
        {
            var descAttr = method.GetCustomAttribute<DescriptionAttribute>();
            var description = descAttr?.Description ?? method.Name;

            logger.LogInformation("[BuildTools] Creating tool: {Name}", method.Name);

            var tool = McpServerTool.Create(
                () => description,
                new McpServerToolCreateOptions
                {
                    Name = method.Name,
                    Description = description,
                    Title = description,
                }
            );
            tools.Add(tool);
        }

        logger.LogInformation("[BuildTools] Built {Count} tools from CodeTools", tools.Count);
        return tools;
    }

    public ValueTask<CallToolResult> CallToolHandler(
        RequestContext<CallToolRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        var result = new CallToolResult();
        var name = request.Params?.Name;
        var args = request.Params?.Arguments;
        logger.LogInformation("[CallTool] Invoking tool: {Name}", name);
        return ValueTask.FromResult(result);
    }
}
