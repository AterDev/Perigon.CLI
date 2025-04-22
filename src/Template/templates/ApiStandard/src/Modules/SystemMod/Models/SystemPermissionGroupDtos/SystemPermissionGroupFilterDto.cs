using Framework.Common.Models;

namespace SystemMod.Models.SystemPermissionGroupDtos;

/// <see cref="Entity.SystemMod.SystemPermissionGroup"/>
public class SystemPermissionGroupFilterDto : FilterBase
{
    /// <summary>
    /// 权限名称标识
    /// </summary>
    [MaxLength(30)]
    public string? Name { get; set; }
}
