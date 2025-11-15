namespace SystemMod.Models.SystemPermissionDtos;

/// <summary>
/// 权限查询筛选
/// </summary>
/// <see cref="SystemPermission"/>
public class SystemPermissionFilterDto : FilterBase
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(30)]
    public string? Name { get; set; }

    /// <summary>
    /// 权限类型
    /// </summary>
    public PermissionType? PermissionType { get; set; }
    public Guid? GroupId { get; set; }
}
