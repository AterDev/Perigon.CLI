namespace SystemMod.Models.SystemPermissionGroupDtos;

/// <see cref="Entity.SystemMod.SystemPermissionGroup"/>
public class SystemPermissionGroupAddDto
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }
    /// <summary>
    /// 权限说明
    /// </summary>
    [MaxLength(length: 1000)]
    public string? Description { get; set; }

}
