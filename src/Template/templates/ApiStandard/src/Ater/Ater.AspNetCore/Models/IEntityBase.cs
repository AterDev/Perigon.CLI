namespace Ater.AspNetCore.Models;

/// <summary>
/// entity base interface
/// </summary>
/// <inheritdoc/>
public interface IEntityBase
{
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
    public DateTimeOffset UpdatedTime { get; set; }
    public bool IsDeleted { get; set; }
}
