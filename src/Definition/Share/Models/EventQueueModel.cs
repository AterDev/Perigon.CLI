namespace Share.Models;

public class EventQueueModel<T>
    where T : class
{
    /// <summary>
    /// 事件名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 数据
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 事件类型
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// 事件时间
    /// </summary>
    public DateTimeOffset DateTime { get; set; }
}
