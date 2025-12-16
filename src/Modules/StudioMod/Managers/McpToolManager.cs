namespace StudioMod.Managers;

public class McpToolManager(
    IDbContextFactory<DefaultDbContext> dbContextFactory,
    ILogger<McpToolManager> logger
) : ManagerBase<DefaultDbContext, McpTool>(dbContextFactory, logger)
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
        return res;
    }
}
