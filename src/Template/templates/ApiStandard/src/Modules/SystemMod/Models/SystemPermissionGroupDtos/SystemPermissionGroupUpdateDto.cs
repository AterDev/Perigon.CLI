namespace SystemMod.Models.SystemPermissionGroupDtos;

/// <see cref="Entity.SystemMod.SystemPermissionGroup"/>
public class SystemPermissionGroupUpdateDto
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(60)]
    public string? Name { get; set; }
    /// <summary>
    /// 权限说明
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }

}
