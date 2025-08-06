using Microsoft.Extensions.Configuration;

namespace AspireHost;

/// <summary>
/// Stores Aspire configuration settings parsed from appsettings.
/// </summary>
public class AspireSetting
{
    public string DatabaseType { get; set; } = "PostgreSQL";
    public string CacheType { get; set; } = "Memory";
    public string CommandDbPassword { get; set; } = string.Empty;
    public string CachePassword { get; set; } = string.Empty;
    public int CommandDbPort { get; set; }
    public int CachePort { get; set; }
    public string? CommandDbConnection { get; set; }
    public string? CacheConnection { get; set; }
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

        var connectionStrings = config.GetSection("ConnectionStrings");
        var commandDbConn = connectionStrings["CommandDb"];
        var cacheConn = connectionStrings["Cache"];

        return new AspireSetting
        {
            DatabaseType = databaseType,
            CacheType = cacheType,
            CommandDbPassword = ExtractPassword(commandDbConn ?? ""),
            CachePassword = ExtractPassword(cacheConn ?? ""),
            CommandDbPort = ExtractPort(commandDbConn ?? "", 5432),
            CachePort = ExtractPort(cacheConn ?? "", 6379),
            CommandDbConnection = commandDbConn,
            CacheConnection = cacheConn,
        };
    }

    public static string ExtractPassword(string connStr)
    {
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return string.Empty;
        }
        var parts = connStr.Split(';');
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (
                kv.Length == 2
                && kv[0].Trim().Equals("Password", StringComparison.OrdinalIgnoreCase)
            )
            {
                return kv[1];
            }
            if (
                kv.Length == 2
                && kv[0].Trim().Equals("password", StringComparison.OrdinalIgnoreCase)
            )
            {
                return kv[1];
            }
        }
        return string.Empty;
    }

    public static int ExtractPort(string connStr, int defaultPort)
    {
        if (string.IsNullOrWhiteSpace(connStr))
        {
            return defaultPort;
        }
        var parts = connStr.Split(';', ',');

        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length == 2 && kv[0].Trim().Equals("Port", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(kv[1], out var port))
                {
                    return port;
                }
            }
            if (kv.Length == 2 && kv[0].Trim().Equals("port", StringComparison.OrdinalIgnoreCase))
            {
                if (int.TryParse(kv[1], out var port))
                {
                    return port;
                }
            }
        }
        return defaultPort;
    }
}
