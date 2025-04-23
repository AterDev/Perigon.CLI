namespace Entity;
/// <summary>
/// 实体基类
/// </summary>
public abstract class EntityBase : IEntityBase
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
}
