namespace StudioMod.Models.GenActionDtos;
/// <summary>
/// 生成操作筛选条件
/// </summary>
/// <see cref="GenAction"/>
public class GenActionFilterDto : FilterBase
{
    /// <summary>
    /// action name
    /// </summary>
    [MaxLength(40)]
    public string? Name { get; set; }
    /// <summary>
    /// source type
    /// </summary>
    public GenSourceType? SourceType { get; set; }
    public Guid? ProjectId { get; set; }
    /// <summary>
    /// 操作状态
    /// </summary>
    public ActionStatus? ActionStatus { get; set; }

}
