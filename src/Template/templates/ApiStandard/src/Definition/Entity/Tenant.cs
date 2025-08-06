namespace Entity;

/// <summary>
/// tenant
/// </summary>
public class Tenant : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? DbConnectionString { get; set; }
}
