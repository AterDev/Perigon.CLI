namespace Share.Models.GenStepDtos;
/// <summary>
/// task step更新时DTO
/// </summary>
/// <see cref="Entity.StudioMod.GenStep"/>
public class GenStepUpdateDto
{
    /// <summary>
    /// 步骤名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
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
    public GenStepType? GenStepType { get; set; }
}
