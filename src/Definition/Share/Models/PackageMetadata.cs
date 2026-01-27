namespace Share.Models;

/// <summary>
/// Module package metadata
/// </summary>
public class PackageMetadata
{
    /// <summary>
    /// Module name
    /// </summary>
    public required string ModuleName { get; set; }

    /// <summary>
    /// Author name
    /// </summary>
    public string? Author { get; set; }

    /// <summary>
    /// Display name
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    public PackageType PackageType { get; set; }

    /// <summary>
    /// Package version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// use the module DI services instead of default services
    /// </summary>
    public bool UseSelfServices { get; set; }

    /// <summary>
    /// Creation time
    /// </summary>
    public DateTime CreatedTime { get; set; } = DateTime.UtcNow;
}
public enum PackageType
{
    /// <summary>
    /// Module package
    /// </summary>
    Module,
    /// <summary>
    /// Service package
    /// </summary>
    Service
}



