namespace SystemMod.Models.SystemMenuDtos;

/// <summary>
/// 系统菜单查询筛选，参数为空时，返回所有菜单树型结构
/// </summary>
/// <inheritdoc cref="SystemMenu"/>
public class SystemMenuFilterDto : FilterBase
{
    /// <summary>
    /// 只显示该层级下菜单
    /// </summary>
    public Guid? ParentId { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public Guid? RoleId { get; set; }
}
