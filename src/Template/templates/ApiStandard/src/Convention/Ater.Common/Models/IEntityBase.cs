namespace Ater.Common.Models;

/// <summary>
/// 数据加基础字段模型
/// </summary>
/// <inheritdoc/>
public interface IEntityBase
{
    public Guid Id { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; }
    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedTime { get; set; }
    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }

}
