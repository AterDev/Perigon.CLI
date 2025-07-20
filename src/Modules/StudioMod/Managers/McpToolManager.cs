namespace StudioMod.Managers;

public class McpToolManager(DataAccessContext<McpTool> dataContext, ILogger<McpToolManager> logger)
    : ManagerBase<McpTool>(dataContext, logger)
{
    /// <summary>
    /// 获取工具列表
    /// </summary>
    /// <returns></returns>
    public async Task<List<McpTool>> ListAsync()
    {
        var tools = await Query.ToListAsync();
        return tools;
    }
}
