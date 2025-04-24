// 系统日志服务示例
using SharedModule;

namespace SystemMod.Services;
/// <summary>
/// 业务日志服务
/// </summary>
public class SystemLogService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEntityTaskQueue<SystemLogs> _taskQueue;
    private readonly IUserContext _context;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <param name="taskQueue"></param>
    public SystemLogService(IServiceProvider serviceProvider, IEntityTaskQueue<SystemLogs> taskQueue)
    {
        _serviceProvider = serviceProvider;
        _taskQueue = taskQueue;
        _context = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<IUserContext>();
    }

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
    public async Task NewLog(string targetName, UserActionType actionType, string description, string? userName = null, Guid? userId = null)
    {
        userId = _context.UserId == Guid.Empty ? userId : _context.UserId;
        userName = string.IsNullOrEmpty(_context.Username) ? userName : _context.Username;
        var route = _context!.GetHttpContext()?.Request.Path.Value;

        if (userId == null || userId.Equals(Guid.Empty))
        {
            return;
        }
        var log = SystemLogs.NewLog(userName ?? "", userId.Value, actionType, targetName, route, description);
        await _taskQueue.AddItemAsync(log);
    }
}
