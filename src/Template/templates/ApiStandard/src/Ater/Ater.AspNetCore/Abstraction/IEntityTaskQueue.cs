using System.Threading.Channels;

namespace Ater.AspNetCore.Abstraction;

public interface IEntityTaskQueue<TEntity>
    where TEntity : class
{
    ValueTask AddItemAsync(TEntity workItem);
    ValueTask<TEntity> DequeueAsync(CancellationToken cancellationToken);
}

/// <summary>
/// 实体队列任务
/// </summary>
public class EntityTaskQueue<TEntity> : IEntityTaskQueue<TEntity>
    where TEntity : class
{
    private readonly Channel<TEntity> _queue;

    public EntityTaskQueue(int capacity = 10000)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
        };
        _queue = Channel.CreateBounded<TEntity>(options);
    }

    public async ValueTask AddItemAsync(TEntity workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<TEntity> DequeueAsync(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);
        return workItem;
    }
}
