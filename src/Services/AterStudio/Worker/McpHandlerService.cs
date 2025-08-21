using Ater.Web.Convention.Abstraction;
using ModelContextProtocol.Server;
using Share.Helper;

namespace AterStudio.Worker;

public class McpHandlerService(
    IMcpServer mcpServer,
    EntityTaskQueue<EventQueueModel<McpTool>> taskQueue
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        OutputHelper.Important("McpHandlerService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var eventModel = await taskQueue.DequeueAsync(stoppingToken);
            if (eventModel != null && eventModel.Data != null)
            {
                var name = eventModel.Name;
                var data = eventModel.Data;
                if (name == "add")
                {
                    AddMcpTool(mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!, data);
                    //await mcpServer.SendNotificationAsync(NotificationMethods.ToolListChangedNotification);
                }
                else if (name == "update")
                {
                    DeleteMcpTool(
                        mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!,
                        data.Name
                    );
                    AddMcpTool(mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!, data);

                    //await mcpServer.SendNotificationAsync(
                    //    NotificationMethods.ToolListChangedNotification
                    //);
                }
                else if (name == "delete")
                {
                    if (
                        mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!.TryGetPrimitive(
                            data.Name.ToString(),
                            out var tool
                        )
                    )
                    {
                        mcpServer.ServerOptions.Capabilities.Tools.ToolCollection!.Remove(tool);
                    }
                }
            }
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        OutputHelper.Important("Stopping McpHandlerService...");
        return base.StopAsync(cancellationToken);
    }

    private static void AddMcpTool(
        McpServerPrimitiveCollection<McpServerTool> collection,
        McpTool tool
    )
    {
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
