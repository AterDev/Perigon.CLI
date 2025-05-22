using Entity.StudioMod;
namespace StudioMod.Models.ProjectDtos;
/// <summary>
/// 项目 ListItem
/// </summary>
/// <see cref="Entity.StudioMod.Project"/>
public class ProjectItemDto
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
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTimeOffset CreatedTime { get; set; }
    
}
