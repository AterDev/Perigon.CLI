namespace StudioMod.Models.GenStepDtos;

/// <summary>
/// task step筛选条件
/// </summary>
/// <see cref="GenStep"/>
public class GenStepFilterDto : FilterBase
{
    public string? Name { get; set; }
    public Guid? ProjectId { get; set; }

    public string? FileType { get; set; }
}
