using Microsoft.Extensions.Configuration;

namespace AspireHost;

/// <summary>
/// Stores Aspire configuration settings parsed from appsettings.
/// </summary>
public class AspireSetting
{
    public string DatabaseType { get; set; } = "PostgreSQL";
    public string CacheType { get; set; } = "Hybrid";
    public string DbPassword { get; set; } = "MyProjectName_Database_Dev";
    public string CachePassword { get; set; } = "MyProjectName_Cache_Dev";
    public int DbPort { get; set; } = 15432;
    public int CachePort { get; set; } = 16379;
}

public static class AppSettingsHelper
{
    /// <summary>
    /// Loads Aspire configuration from appsettings and parses required values.
    /// </summary>
    /// <param name="environment">The environment name, e.g. "Development".</param>
    /// <returns>AspireSetting instance with parsed values.</returns>
    public static AspireSetting LoadAspireSettings(IConfiguration config)
    {
        var components = config.GetSection("Components");
        var databaseType = components["Database"] ?? "PostgreSQL";
        var cacheType = components["Cache"] ?? "Memory";

        return new AspireSetting
        {
            DatabaseType = databaseType,
            CacheType = cacheType,
            DbPort = databaseType.ToLowerInvariant() switch
            {
                "postgresql" => 15432,
                "sqlserver" => 11433,
                _ => 13306,
            },
        };
    }
}
