namespace Entity.CommonMod;

/// <summary>
/// The tenant entity
/// </summary>
[Index(nameof(Domain), IsUnique = true)]
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
    public required string Domain { get; set; }

    public TenantType Type { get; set; } = TenantType.Normal;

    public bool Disabled { get; set; }
}

public enum TenantType
{
    /// <summary>
    /// Trial
    /// </summary>
    [Description("Trial")]
    Trial,

    /// <summary>
    /// Normal
    /// </summary>
    [Description("Normal")]
    Normal,

    /// <summary>
    /// Independent
    /// </summary>
    [Description("Independent")]
    Independent,
}
