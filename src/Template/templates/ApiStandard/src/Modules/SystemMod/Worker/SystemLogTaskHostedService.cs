using System.Reflection;
using Ater.AspNetCore.Attributes;
using Microsoft.Extensions.Hosting;

namespace SystemMod.Worker;

/// <summary>
/// 日志记录任务
/// </summary>
public class SystemLogTaskHostedService(
    IServiceProvider serviceProvider,
    IEntityTaskQueue<SystemLogs> queue,
    ILogger<SystemLogTaskHostedService> logger
) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly ILogger<SystemLogTaskHostedService> _logger = logger;
    private readonly IEntityTaskQueue<SystemLogs> _taskQueue = queue;

    private readonly int MaxWaitMilliseconds = 10_000;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"🚀 System Log Hosted Service is running.");
        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        List<SystemLogs> logs = [];
        DateTime? batchStart = null;

        while (!stoppingToken.IsCancellationRequested)
        {
            var log = await _taskQueue.DequeueAsync(stoppingToken);
            batchStart ??= DateTime.Now;
            logs.Add(log);
            if (
                logs.Count >= 10
                || (DateTime.Now - batchStart.Value).TotalMilliseconds > MaxWaitMilliseconds
            )
            {
                await InsertLogsAsync(logs, stoppingToken);
                logs.Clear();
                batchStart = null;
            }
        }

        // 插入剩余的日志
        if (logs.Count > 0)
        {
            await InsertLogsAsync(logs, stoppingToken);
        }
    }

    private async Task InsertLogsAsync(List<SystemLogs> logs, CancellationToken stoppingToken)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();

        foreach (var log in logs)
        {
            var entity = log.Data;
            if (entity is string)
            {
                log.TargetName = entity as string;
            }
            else if (entity != null)
            {
                var type = entity.GetType();
                var attribute = type?.GetCustomAttribute<LogDescriptionAttribute>();
                if (attribute != null)
                {
                    log.TargetName = attribute.Description;
                    log.Description ??= log.ActionType + attribute.Description;
                    if (attribute.FieldName != null)
                    {
                        var fieldValue = type!.GetProperty(attribute.FieldName)?.GetValue(entity);
                        if (fieldValue != null)
                        {
                            log.TargetName = fieldValue as string;
                        }
                    }
                }
            }
        }

        try
        {
            context.AddRange(logs);
            await context.SaveChangesAsync(stoppingToken);
            foreach (var log in logs)
            {
                _logger.LogInformation(
                    "✍️ New Log:[{object}] {actionUser} {action} {name}",
                    log.Description,
                    log.ActionUserName,
                    log.ActionType,
                    log.TargetName
                );
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred executing batch logs. Count: {count}", logs.Count);
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🛑 System Log Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
