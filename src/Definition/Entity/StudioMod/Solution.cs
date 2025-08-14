namespace Entity.StudioMod;

/// <summary>
/// 项目
/// </summary>
[Module(Modules.Studio)]
public class Solution : EntityBase
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 显示名
    /// </summary>
    [MaxLength(100)]
    public required string DisplayName { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(200)]
    public required string Path { get; set; }

    /// <summary>
    /// 版本
    /// </summary>
    [MaxLength(20)]
    public string? Version { get; set; }

    /// <summary>
    /// 解决方案类型
    /// </summary>
    public SolutionType? SolutionType { get; set; }

    /// <summary>
    /// project config
    /// </summary>
    public SolutionConfig Config { get; set; } = new SolutionConfig();

    public List<ApiDocInfo> ApiDocInfos { get; set; } = [];

    public ICollection<GenAction> GenActions { get; set; } = [];

    public ICollection<GenStep> GenSteps { get; set; } = [];

    public ICollection<McpTool> McpTools { get; set; } = [];
}

/// <summary>
///  项目配置
/// </summary>
public class SolutionConfig
{
    public string IdType { get; set; } = ConstVal.Guid;
    public string CreatedTimeName { get; set; } = ConstVal.CreatedTime;
    public string UpdatedTimeName { get; set; } = ConstVal.UpdatedTime;
    public string Version { get; set; } = ConstVal.Version;
    public string SharePath { get; set; } = PathConst.SharePath;
    public string EntityPath { get; set; } = PathConst.EntityPath;
    public string EntityFrameworkPath { get; set; } = PathConst.EntityFrameworkPath;
    public string CommonModPath { get; set; } = PathConst.CommonModPath;
    public string ModulePath { get; set; } = PathConst.ModulesPath;
    public string ApiPath { get; set; } = PathConst.APIPath;
    public string ServicePath { get; set; } = PathConst.ServicesPath;
    public string SolutionPath { get; set; } = string.Empty;

    /// <summary>
    /// 是否为租户模式
    /// </summary>
    public bool IsTenantMode { get; set; }

    /// <summary>
    /// 用来标识哪些为用户Id
    /// </summary>
    public List<string> UserIds { get; set; } = ["UserId", "SystemUserId", "AccountId"];
}

public enum SolutionType
{
    [Description("DotNet")]
    DotNet,

    [Description("Node")]
    Node,

    [Description("Else")]
    Else,
}
