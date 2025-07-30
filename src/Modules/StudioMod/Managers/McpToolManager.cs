using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace StudioMod.Managers;

public class McpToolManager(
    DataAccessContext<McpTool> dataContext,
    IServiceScopeFactory factory,
    ILogger<McpToolManager> logger
) : ManagerBase<McpTool>(dataContext, logger)
{
    /// <summary>
    /// 获取工具列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<McpTool>> ListAsync()
    {
        var tools = await ToListAsync();
        return tools;
    }

    public override Task<bool> ExistAsync(Guid id)
    {
        return base.ExistAsync(id);
    }

    /// <summary>
    /// add mcp tool
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public new async Task<bool> AddAsync(McpTool entity)
    {
        var res = await base.AddAsync(entity);
        if (res)
        {
            var mcpServer = factory.CreateScope().ServiceProvider.GetService<IMcpServer>();
            // 动态添加工具
            if (mcpServer == null)
            {
                return true;
            }
            mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!.Add(
                McpServerTool.Create(
                    () =>
                    {
                        var prompt = $"""
                        根据以下 prompt和 template 内容生成代码，下面将提供prompt和template的本地路径：
                            <prompt>
                            {entity.PromptPath}
                            </prompt>
                            <template>
                            {string.Join(Environment.NewLine, entity.TemplatePaths)}
                            </template>
                        """;
                    },
                    new McpServerToolCreateOptions
                    {
                        Name = entity.Name,
                        Description = entity.Description,
                        Title = entity.Description,
                    }
                )
            );
            await mcpServer.SendNotificationAsync(NotificationMethods.ToolListChangedNotification);
        }
        return res;
    }

    /// <summary>
    /// update mcp tool
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    public new async Task<bool> UpdateAsync(McpTool entity)
    {
        var res = await base.UpdateAsync(entity);

        if (res)
        {
            var mcpServer = factory.CreateScope().ServiceProvider.GetService<IMcpServer>();
            if (mcpServer == null)
            {
                return true;
            }
            // 动态更新工具
            if (
                mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!.TryGetPrimitive(
                    entity.Name,
                    out var tool
                )
            )
            {
                mcpServer.ServerOptions.Capabilities.Tools.ToolCollection!.Remove(tool);
            }
            mcpServer.ServerOptions.Capabilities.Tools.ToolCollection!.Add(
                McpServerTool.Create(
                    () =>
                    {
                        var prompt = $"""
                        根据以下 prompt和 template 内容生成代码，下面将提供prompt和tempalte的本地路径：
                            <prompt>
                            {entity.PromptPath}
                            </prompt>
                            <template>
                            {string.Join(Environment.NewLine, entity.TemplatePaths)}
                            </template>
                        """;
                    },
                    new McpServerToolCreateOptions
                    {
                        Name = entity.Name,
                        Description = entity.Description,
                        Title = entity.Description,
                    }
                )
            );
            await mcpServer.SendNotificationAsync(NotificationMethods.ToolListChangedNotification);
        }
        return res;
    }

    /// <summary>
    /// delete mcp tool
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<bool> DeleteAsync(Guid id)
    {
        var res = await base.DeleteAsync([id]);
        if (res)
        {
            var mcpServer = factory.CreateScope().ServiceProvider.GetService<IMcpServer>();
            if (mcpServer == null)
            {
                return true;
            }
            // 动态删除工具
            if (
                mcpServer.ServerOptions.Capabilities!.Tools!.ToolCollection!.TryGetPrimitive(
                    id.ToString(),
                    out var tool
                )
            )
            {
                mcpServer.ServerOptions.Capabilities.Tools.ToolCollection!.Remove(tool);
            }
            await mcpServer.SendNotificationAsync(NotificationMethods.ToolListChangedNotification);
        }
        return res;
    }
}
