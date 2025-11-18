namespace Entity;

/// <summary>
/// 实体基类
/// </summary>
public abstract class EntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; private set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
    public bool IsDeleted { get; set; }
    public Guid TenantId { get; set; }
}
