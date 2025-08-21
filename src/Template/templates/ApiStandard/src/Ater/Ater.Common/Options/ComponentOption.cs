namespace Ater.Common.Options;

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

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public AuthType AuthType { get; set; } = AuthType.Jwt;

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public MQType MQType { get; set; } = MQType.None;
    public bool UseCors { get; set; } = true;
    public bool UseSMS { get; set; }
    public bool UseSmtp { get; set; }
    public bool UseAWSS3 { get; set; }
    public bool UseOpenAPI { get; set; } = true;
}

public enum AuthType
{
    Jwt,
    Cookie,
    OAuth,
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
    Hybrid,
}

public enum MQType
{
    None,
    RabbitMQ,
    Kafka,
}
