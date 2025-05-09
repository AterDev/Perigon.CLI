using Framework.Common.Utils;

namespace Entity.StudioMod;
/// <summary>
/// task step
/// </summary>
public class GenStep : EntityBase
{
    /// <summary>
    /// 步骤名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 模板或命令内容
    /// </summary>
    [MaxLength(100_000)]
    public string? Content { get; set; }

    /// <summary>
    /// 生成内容
    /// </summary>
    [MaxLength(100_000)]
    public string? OutputContent { get; set; }

    /// <summary>
    /// 模板或脚本路径
    /// </summary>
    [MaxLength(400)]
    public string? Path { get; set; }

    /// <summary>
    /// 输出路径
    /// </summary>
    [MaxLength(400)]
    public string? OutputPath { get; set; }

    /// <summary>
    /// step type
    /// </summary>
    public GenStepType GenStepType { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [MaxLength(20)]
    public string? FileType { get; set; }

    public ICollection<GenAction> GenActions { get; set; } = [];

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;
    public Guid ProjectId { get; set; } = default!;

    /// <summary>
    /// 格式化路径
    /// </summary>
    /// <param name="variables"></param>
    /// <returns></returns>
    public string OutputPathFormat(List<Variable> variables)
    {
        string format = OutputPath ?? string.Empty;
        if (format.NotEmpty())
        {
            // 循环将vriables中的key 匹配的@{key}替换 成value
            foreach (var variable in variables)
            {
                format = format.Replace($"@{{{variable.Key}}}", variable.Value);
            }
        }
        return format;
    }
}

/// <summary>
/// step type
/// </summary>
public enum GenStepType
{
    /// <summary>
    /// 模板生成
    /// </summary>
    [Description("模板生成")]
    File,
    /// <summary>
    /// 运行命令
    /// </summary>
    [Description("执行命令")]
    Command,
    /// <summary>
    /// 运行脚本
    /// </summary>
    [Description("执行脚本")]
    Script
}
