using Entity.StudioMod;
namespace StudioMod.Models.ProjectDtos;
/// <summary>
/// 项目 UpdateDTO
/// </summary>
/// <see cref="Entity.StudioMod.Project"/>
public class ProjectUpdateDto
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    /// <summary>
    /// 显示名
    /// </summary>
    [MaxLength(100)]
    public string? DisplayName { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(200)]
    public string? Path { get; set; }
    /// <summary>
    /// 版本
    /// </summary>
    [MaxLength(20)]
    public string? Version { get; set; }
    /// <summary>
    /// 解决方案类型
    /// </summary>
    public SolutionType? SolutionType { get; set; }
    /// <summary>
    /// project config
    /// </summary>
    public ProjectConfig? Config { get; set; }
    public List<Guid>? ApiDocInfoIds { get; set; }
    public List<Guid>? GenActionIds { get; set; }
    public List<Guid>? GenStepIds { get; set; }
    
}
