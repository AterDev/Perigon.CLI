using Entity.StudioMod;
namespace StudioMod.Models.ProjectDtos;
/// <summary>
/// 项目 AddDto
/// </summary>
/// <see cref="Entity.StudioMod.Project"/>
public class ProjectAddDto
{
    /// <summary>
    /// 项目名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 显示名
    /// </summary>
    [MaxLength(100)]
    public string DisplayName { get; set; } = default!;
    /// <summary>
    /// 路径
    /// </summary>
    [MaxLength(200)]
    public string Path { get; set; } = default!;
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
    public ProjectConfig Config { get; set; } = new ProjectConfig();
    
}
