using Entity.SystemMod;

namespace SystemMod.Models.SystemPermissionDtos;
/// <summary>
/// 权限更新时请求结构
/// </summary>
/// <see cref="SystemPermission"/>
public class SystemPermissionUpdateDto
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(30)]
    public string Name { get; set; } = default!;
    /// <summary>
    /// 权限说明
    /// </summary>
    [MaxLength(200)]
    public string? Description { get; set; }
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? Enable { get; set; }
    /// <summary>
    /// 权限类型
    /// </summary>
    public PermissionType? PermissionType { get; set; }
    public Guid? SystemPermissionGroupId { get; set; }

}
