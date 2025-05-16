using System.ComponentModel;

namespace Share.Models.CommandDtos;

/// <summary>
/// 创建解决方案dto
/// </summary>
public class CreateSolutionDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(300)]
    public required string Path { get; set; }

    /// <summary>
    /// 是否为轻量模板
    /// </summary>
    public bool IsLight { get; set; }

    /// <summary>
    /// 数据库类型
    /// </summary>
    public DBType DBType { get; set; } = DBType.PostgreSQL;

    /// <summary>
    /// 缓存类型
    /// </summary>
    public CacheType CacheType { get; set; } = CacheType.Redis;

    /// <summary>
    /// 前端项目
    /// </summary>
    public FrontType FrontType { get; set; } = FrontType.None;

    /// <summary>
    /// 是否包含租户
    /// </summary>
    public bool HasTenant { get; set; }

    /// <summary>
    /// 是否包含验证授权服务
    /// </summary>
    public bool HasIdentityServer { get; set; }
    /// <summary>
    /// 是否包含任务管理服务
    /// </summary>
    public bool HasTaskManager { get; set; }
    /// <summary>
    /// 写数据库连接字符串
    /// </summary>
    [MaxLength(300)]
    public string? CommandDbConnStrings { get; set; }
    /// <summary>
    /// 读数据库连接字符串
    /// </summary>
    [MaxLength(300)]
    public string? QueryDbConnStrings { get; set; }
    /// <summary>
    /// 缓存连接字符串
    /// </summary>
    [MaxLength(200)]
    public string? CacheConnStrings { get; set; }
    /// <summary>
    /// 缓存实例名称
    /// </summary>
    [MaxLength(60)]
    public string? CacheInstanceName { get; set; }

    [MaxLength(60)]
    public string? DefaultPassword { get; set; }

    /// <summary>
    /// 选择的模块
    /// </summary>
    public List<string> Modules { get; set; } = [];
}

public enum DBType
{
    /// <summary>
    /// PostgreSQL
    /// </summary>
    [Description("PostgreSQL")]
    PostgreSQL,
    /// <summary>
    /// SQLServer
    /// </summary>
    [Description("SQLServer")]
    SQLServer,
}
public enum CacheType
{
    /// <summary>
    /// Redis
    /// </summary>
    [Description(description: "Redis")]
    Redis,
    /// <summary>
    /// Memory
    /// </summary>
    [Description(description: "Memory")]
    Memory
}

/// <summary>
/// 前端项目
/// </summary>
public enum FrontType
{
    /// <summary>
    /// 无
    /// </summary>
    None,
    /// <summary>
    /// Angular
    /// </summary>
    [Description("Angular")]
    Angular,
    /// <summary>
    /// Blazor
    /// </summary>
    [Description("Blazor")]
    Blazor,
}