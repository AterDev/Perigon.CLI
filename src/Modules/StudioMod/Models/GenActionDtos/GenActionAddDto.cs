namespace StudioMod.Models.GenActionDtos;
/// <summary>
/// 生成操作添加时DTO
/// </summary>
/// <see cref="GenAction"/>
public class GenActionAddDto
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public required string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    /// <summary>
    /// 实体路径
    /// </summary>
    public string? EntityPath { get; set; }
    /// <summary>
    /// open api path
    /// </summary>
    public string? OpenApiPath { get; set; }
    public List<Variable> Variables { get; set; } = [];
    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType? SourceType { get; set; }
    /// <summary>
    /// 操作状态
    /// </summary>
    public ActionStatus ActionStatus { get; set; } = ActionStatus.NotStarted;

}
