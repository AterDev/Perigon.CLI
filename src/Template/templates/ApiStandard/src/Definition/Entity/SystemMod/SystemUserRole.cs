namespace Entity.SystemMod;

/// <summary>
/// 系统用户角色关联表
/// </summary>
[Index(nameof(UserId), nameof(RoleId), IsUnique = true)]
[Index(nameof(UserId))]
[Index(nameof(RoleId))]
public class SystemUserRole : EntityBase
{
    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色ID
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 导航属性 - 用户
    /// </summary>
    public SystemUser User { get; set; } = default!;

    /// <summary>
    /// 导航属性 - 角色
    /// </summary>
    public SystemRole Role { get; set; } = default!;
}
