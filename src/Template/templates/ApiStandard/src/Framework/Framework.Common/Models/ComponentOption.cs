using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Common.Models;

/// <summary>
/// 组件配置
/// </summary>
public class ComponentOption
{
    public const string ConfigPath = "Components";

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public CacheType Cache { get; set; } = CacheType.Memory;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public DatabaseType Database { get; set; } = DatabaseType.PostgreSql;

    public bool UseCORS { get; set; } = true;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuthType AuthType { get; set; } = AuthType.Jwt;
}


public enum AuthType
{
    Jwt,
    Cookie,
    OAuth
}
public enum DatabaseType
{
    SqlServer,
    PostgreSql,
}
public enum CacheType
{
    Memory,
    Redis,
    Hybrid
}