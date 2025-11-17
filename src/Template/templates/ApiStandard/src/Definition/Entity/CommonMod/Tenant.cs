namespace Entity.CommonMod;

/// <summary>
/// The tenant entity
/// </summary>
[Index(nameof(Name), IsUnique = true)]
public class Tenant : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    [MaxLength(500)]
    public string? DbConnectionString { get; set; }

    [MaxLength(500)]
    public string? AnalysisConnectionString { get; set; }

    [MaxLength(200)]
    public string? Domain { get; set; }

    public bool Enable { get; set; }
}
