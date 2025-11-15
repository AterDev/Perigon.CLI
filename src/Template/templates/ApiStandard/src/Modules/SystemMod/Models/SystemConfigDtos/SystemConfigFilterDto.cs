namespace SystemMod.Models.SystemConfigDtos;

/// <summary>
/// 系统配置查询筛选
/// </summary>
/// <see cref="SystemConfig"/>
public class SystemConfigFilterDto : FilterBase
{
    [MaxLength(100)]
    public string? Key { get; set; }

    /// <summary>
    /// 组
    /// </summary>
    public string? GroupName { get; set; }
}
