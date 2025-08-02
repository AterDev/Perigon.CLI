using Ater.Common.Utils;

namespace Entity.StudioMod;

/// <summary>
/// template
/// </summary>
public class GenStep : EntityBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    /// <summary>
    /// 生成内容
    /// </summary>
    [MaxLength(100_000)]
    public string? OutputContent { get; set; }

    /// <summary>
    /// 模板路径
    /// </summary>
    [MaxLength(400)]
    public string? TemplatePath { get; set; }

    /// <summary>
    /// 输出路径
    /// </summary>
    [MaxLength(400)]
    public string? OutputPath { get; set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    [MaxLength(20)]
    public string? FileType { get; set; }

    public ICollection<GenAction> GenActions { get; set; } = [];

    [ForeignKey(nameof(ProjectId))]
    public Solution Project { get; set; } = null!;
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
