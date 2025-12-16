namespace Entity;

/// <summary>
/// entity base interface
/// </summary>
public interface IEntityBase
{
    Guid Id { get; set; }
    DateTimeOffset CreatedTime { get; }
    DateTimeOffset UpdatedTime { get; set; }
    bool IsDeleted { get; set; }
}

