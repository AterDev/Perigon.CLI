namespace Share.Models.GenStepDtos;
/// <summary>
/// task step列表元素
/// </summary>
/// <see cref="Entity.StudioMod.GenStep"/>
public class GenStepItemDto
{
    /// <summary>
    /// 步骤名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 输出路径
    /// </summary>
    [MaxLength(400)]
    public string? OutputPath { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(400)]
    public string? Path { get; set; }
    /// <summary>
    /// step type
    /// </summary>
    public GenStepType GenStepType { get; set; }
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTimeOffset CreatedTime { get; set; }

}
