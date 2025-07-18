namespace Entity.StudioMod;

/// <summary>
/// 生成操作
/// </summary>
[Index(nameof(Name))]
[Index(nameof(Description))]
public class GenAction : EntityBase
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// 实体/模型路径
    /// </summary>
    [MaxLength(1024)]
    public string? EntityPath { get; set; }

    /// <summary>
    /// open api path
    /// </summary>
    [MaxLength(1024)]
    public string? OpenApiPath { get; set; }

    public List<Variable> Variables { get; set; } = [];

    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType SourceType { get; set; }

    /// <summary>
    /// action step
    /// </summary>
    public ICollection<GenStep> GenSteps { get; set; } = [];

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;

    /// <summary>
    /// 操作状态
    /// </summary>
    public ActionStatus ActionStatus { get; set; } = ActionStatus.NotStarted;
}

public enum ActionStatus
{
    /// <summary>
    /// 未开始
    /// </summary>
    [Description("未执行")]
    NotStarted,

    /// <summary>
    /// 进行中
    /// </summary>
    [Description("执行中")]
    InProgress,

    /// <summary>
    /// 已完成
    /// </summary>
    [Description("成功")]
    Success,

    /// <summary>
    /// 已失败
    /// </summary>
    [Description("失败")]
    Failed,
}

/// <summary>
/// Source Type
/// </summary>
public enum GenSourceType
{
    /// <summary>
    /// 实体类
    /// </summary>
    [Description("实体类")]
    EntityClass,

    /// <summary>
    /// dto模型
    /// </summary>
    [Description("Dto模型")]
    DtoModel,

    /// <summary>
    /// OpenAPI
    /// </summary>
    [Description("OpenAPI")]
    OpenAPI,
}

public class Variable
{
    [MaxLength(100)]
    public required string Key { get; set; }

    [MaxLength(1000)]
    public required string Value { get; set; }
}
