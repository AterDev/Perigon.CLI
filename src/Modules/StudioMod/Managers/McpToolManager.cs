using Ater.Web.Convention.Abstraction;

namespace StudioMod.Managers;

public class McpToolManager(
    DefaultDbContext dbContext,
    EntityTaskQueue<EventQueueModel<McpTool>> taskQueue,
    ILogger<McpToolManager> logger
) : ManagerBase<DefaultDbContext, McpTool>(dbContext, logger)
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
            await taskQueue.AddItemAsync(
                new EventQueueModel<McpTool> { Name = "add", Data = entity }
            );
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
            await taskQueue.AddItemAsync(
                new EventQueueModel<McpTool> { Name = "update", Data = entity }
            );
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
        var entity = await FindAsync(id);
        var res = await base.DeleteAsync([id]);
        if (res)
        {
            await taskQueue.AddItemAsync(
                new EventQueueModel<McpTool> { Name = "delete", Data = entity }
            );
        }
        return res;
    }
}
