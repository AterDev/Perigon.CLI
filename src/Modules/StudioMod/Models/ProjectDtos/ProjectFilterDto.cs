using Entity.StudioMod;
namespace StudioMod.Models.ProjectDtos;
/// <summary>
/// 项目 Filter
/// </summary>
/// <see cref="Entity.StudioMod.Project"/>
public class ProjectFilterDto : FilterBase
{
    /// <summary>
    /// 版本
    /// </summary>
    [MaxLength(20)]
    public string? Version { get; set; }
    /// <summary>
    /// 解决方案类型
    /// </summary>
    public SolutionType? SolutionType { get; set; }
    
}
