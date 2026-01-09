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
    public string Name { get; set; } = string.Empty;

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

    /// <summary>
    /// project id
    /// </summary>
    public int ProjectId { get; set; }

    /// <summary>
    /// 格式化路径
    /// </summary>
    /// <param name="variables"></param>
    /// <returns></returns>
    public string OutputPathFormat(List<Variable> variables)
    {
        string format = OutputPath ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(format))
        {
            foreach (var variable in variables)
            {
                format = format.Replace($"@{{{variable.Key}}}", variable.Value);
            }
        }
        return format;
    }
}
