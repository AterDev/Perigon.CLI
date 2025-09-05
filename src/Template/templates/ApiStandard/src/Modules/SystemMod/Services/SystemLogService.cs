// 系统日志服务示例
namespace SystemMod.Services;

/// <summary>
/// 业务日志服务
/// </summary>
/// <remarks>
/// 系统日志服务
/// </remarks>
/// <param name="serviceProvider"></param>
/// <param name="taskQueue"></param>
public class SystemLogService(
    IServiceProvider serviceProvider,
    IEntityTaskQueue<SystemLogs> taskQueue
)
{
    /// <summary>
    /// 记录日志,优先从UserContext中获取用户信息
    /// 匿名用户访问，需要传入userName和userId
    /// </summary>
    /// <param name="targetName"></param>
    /// <param name="actionType"></param>
    /// <param name="description"></param>
    /// <param name="userName"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task NewLog(
        string targetName,
        UserActionType actionType,
        string description,
        string? userName = null,
        Guid? userId = null
    )
    {
        var _context = serviceProvider
            .CreateScope()
            .ServiceProvider.GetRequiredService<IUserContext>();

        userId = _context.UserId == Guid.Empty ? userId : _context.UserId;
        userName = string.IsNullOrEmpty(_context.UserName) ? userName : _context.UserName;
        var route = _context!.HttpContext?.Request.Path.Value;

        if (userId == null || userId.Equals(Guid.Empty))
        {
            return;
        }
        var log = SystemLogs.NewLog(
            userName ?? "",
            userId.Value,
            targetName,
            actionType,
            route,
            description
        );
        await taskQueue.AddItemAsync(log);
    }
}
