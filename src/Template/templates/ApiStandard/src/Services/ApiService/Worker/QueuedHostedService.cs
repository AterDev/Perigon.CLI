namespace ApiService.Worker;
/// <summary>
/// 后台队列任务服务示例
/// </summary>
public class QueuedHostedService(IBackgroundTask taskQueue,
    ILogger<QueuedHostedService> logger) : BackgroundService
{
    private readonly ILogger<QueuedHostedService> _logger = logger;

    public IBackgroundTask TaskQueue { get; } = taskQueue;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation($"Queued Hosted Service is running.");

        await BackgroundProcessing(stoppingToken);
    }

    private async Task BackgroundProcessing(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            Func<object, ValueTask> workItem = await TaskQueue.DequeueAsync(stoppingToken);

            try
            {
                Console.WriteLine("task run");
                await workItem(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error occurred executing {WorkItem}.", nameof(workItem));
            }
        }
    }

    public override async Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queued Hosted Service is stopping.");
        await base.StopAsync(stoppingToken);
    }
}
