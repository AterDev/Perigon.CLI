using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.Text;

namespace AterStudio.McpTools;

public class McpToolsHandler(DefaultDbContext dbContext)
{
    public async ValueTask<ListToolsResult> ListToolsHandler(
        RequestContext<ListToolsRequestParams> request,
        CancellationToken cancellationToken
    )
    {
        var result = new ListToolsResult();
        var defaultTools = request.Server.ServerOptions.ToolCollection ?? [];
        var tools = dbContext.McpTools.ToList();

        foreach (var tool in tools)
        {

            result.Tools.Add(new Tool
            {
                Name = tool.Name,
                Description = tool.Description,
                Title = tool.Description,

            });
            //AddMcpTool(defaultTools, tool);
        }
        //foreach (var tool in defaultTools)
        //{
        //    result.Tools.Add(
        //        new Tool
        //        {
        //            Name = tool.ProtocolTool.Name,
        //            Description = tool.ProtocolTool.Description,
        //            Title = tool.ProtocolTool.Title,
        //        }
        //    );
        //}
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

    private static McpServerTool CreateMcpTool(McpTool tool)
    {
        return McpServerTool.Create(
               () =>
               {
                   var promptContent = File.ReadAllText(tool.PromptPath);
                   var templates = tool.TemplatePaths
                       .Select(s => new
                       {
                           name = Path.GetFileName(s),
                           content = File.ReadAllText(s)
                       });

                   var sb = new StringBuilder();
                   if (templates.Any())
                   {
                       foreach (var template in templates)
                       {
                           sb.AppendLine($"## {template.name}");
                           sb.AppendLine(template.content);
                       }
                   }

                   var prompt = $"""
                    {promptContent}，提供良好的代码格式和规范。
                    <example>
                    {sb}
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
           );
    }
}
