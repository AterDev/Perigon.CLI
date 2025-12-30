using System.Text.Json;

namespace Entity.StudioMod;

/// <summary>
/// 生成操作
/// </summary>
public class GenAction : EntityBase
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    [Required]
    public string Name { get; set; } = string.Empty;

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

    /// <summary>
    /// Variables stored as JSON string
    /// </summary>
    [MaxLength(5000)]
    public string VariablesJsonString { get; set; } = string.Empty;

    /// <summary>
    /// action variables
    /// </summary>
    [NotMapped]
    public List<Variable> Variables
    {
        get
        {
            if (string.IsNullOrEmpty(VariablesJsonString))
                return [];

            return JsonSerializer.Deserialize<List<Variable>>(VariablesJsonString) ?? [];
        }
        set
        {
            if (value == null || value.Count == 0)
                VariablesJsonString = string.Empty;
            else
                VariablesJsonString = JsonSerializer.Serialize(value);
        }
    }

    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType SourceType { get; set; }

    /// <summary>
    /// project id
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// 操作状态
    /// </summary>
    public ActionStatus ActionStatus { get; set; } = ActionStatus.NotStarted;

    /// <summary>
    /// 关联的步骤
    /// </summary>
    [NotMapped]
    public List<GenStep> GenSteps { get; set; } = [];
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
    public string Key { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Value { get; set; } = string.Empty;
}
