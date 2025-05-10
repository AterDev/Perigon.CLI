namespace Share.Models.GenStepDtos;
/// <summary>
/// task step筛选条件
/// </summary>
/// <see cref="Entity.StudioMod.GenStep"/>
public class GenStepFilterDto : FilterBase
{
    public string? Name { get; set; }
    /// <summary>
    /// step type
    /// </summary>
    public GenStepType? GenStepType { get; set; }
    public Guid? ProjectId { get; set; }

    public string? FileType { get; set; }
}
