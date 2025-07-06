namespace Entity.StudioMod;

/// <summary>
/// 项目
/// </summary>
[Module(Modules.Studio)]
public class Project : EntityBase
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
    public ProjectConfig Config { get; set; } = new ProjectConfig();

    public List<ApiDocInfo> ApiDocInfos { get; set; } = [];

    public ICollection<GenAction> GenActions { get; set; } = [];

    public ICollection<GenStep> GenSteps { get; set; } = [];
}

/// <summary>
///  项目配置
/// </summary>
public class ProjectConfig
{
    public string IdType { get; set; } = ConstVal.Guid;
    public string CreatedTimeName { get; set; } = ConstVal.CreatedTime;
    public string UpdatedTimeName { get; set; } = ConstVal.UpdatedTime;
    public string Version { get; set; } = ConstVal.Version;
    public string SharePath { get; set; } = PathConst.SharePath;
    public string EntityPath { get; set; } = PathConst.EntityPath;
    public string EntityFrameworkPath { get; set; } = PathConst.EntityFrameworkPath;
    public string CommonModPath { get; set; } = PathConst.CommonModPath;
    public string ApiPath { get; set; } = PathConst.APIPath;
    public string MicroservicePath { get; set; } = PathConst.MicroservicePath;
    public string SolutionPath { get; set; } = "";
    public bool IsLight { get; private set; } = false;
    public ControllerType ControllerType { get; set; } = ControllerType.Client;

    /// <summary>
    /// 是否拆分
    /// </summary>
    public bool? IsSplitController { get; set; } = false;
}

public enum ControllerType
{
    /// <summary>
    /// 用户端
    /// </summary>
    [Description("用户端")]
    Client,

    /// <summary>
    /// 管理端
    /// </summary>
    [Description("管理端")]
    Admin,

    /// <summary>
    /// 用户和管理端
    /// </summary>
    [Description("用户端和管理端")]
    Both,
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
