namespace Entity.StudioMod;
/// <summary>
/// 操作模板
/// </summary>
public class GenActionTpl : EntityBase
{
    /// <summary>
    /// 模板名称
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }

    /// <summary>
    /// 模板说明
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }

    /// <summary>
    /// GenAction Json Content
    /// </summary>
    [MaxLength(1000_1024)]
    public required string ActionContent { get; set; }
}
