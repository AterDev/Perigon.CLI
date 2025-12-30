using Perigon.MiniDb;

namespace Entity;

public abstract class EntityBase : IMicroEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
}
