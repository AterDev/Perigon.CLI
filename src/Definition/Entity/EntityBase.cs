namespace Entity;

public abstract class EntityBase : IEntityBase
{
    [Key]
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
}
