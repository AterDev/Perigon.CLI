using DataContext.DBProvider;
using Microsoft.Extensions.Hosting;

namespace AterStudio.Worker;

public class InitDataTask(DefaultDbContext context) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // 初始化数据库
        // MiniDb不需要migration，但我们可以在这里初始化数据
        await InitializeDataAsync();
    }

    private async Task InitializeDataAsync()
    {
        // 自定义初始化逻辑
        // 例如：创建默认配置、添加示例数据等
        await Task.CompletedTask;
    }
}
