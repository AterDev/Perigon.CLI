using System.Threading.Channels;

namespace Ater.AspNetCore.Abstraction;

public interface IBackgroundTask
{
    /// <summary>
    /// 向通道中添加任务
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    ValueTask QueueBackgroundWorkItemAsync(Func<object, ValueTask> workItem);

    /// <summary>
    /// 从通道中获取任务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    ValueTask<Func<object, ValueTask>> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// 后台队列定义
/// </summary>
public class BackgroundTask : IBackgroundTask
{
    private readonly Channel<Func<object, ValueTask>> _queue;

    public BackgroundTask(int capacity = 1000)
    {
        BoundedChannelOptions options = new(capacity) { FullMode = BoundedChannelFullMode.Wait };
        // 创建一个有容量的通道，可根据实体需求进行调整
        _queue = Channel.CreateBounded<Func<object, ValueTask>>(options);
    }

    /// <summary>
    /// 向通道中添加任务
    /// </summary>
    /// <param name="workItem"></param>
    /// <returns></returns>
    public async ValueTask QueueBackgroundWorkItemAsync(Func<object, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _queue.Writer.WriteAsync(workItem);
    }

    /// <summary>
    /// 从通道中获取任务
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async ValueTask<Func<object, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken
    )
    {
        Func<object, ValueTask> workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
