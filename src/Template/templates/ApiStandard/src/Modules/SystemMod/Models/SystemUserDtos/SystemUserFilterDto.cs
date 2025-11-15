namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 系统用户查询筛选
/// </summary>
/// <inheritdoc cref="SystemUser"/>
public class SystemUserFilterDto : FilterBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 角色id
    /// </summary>
    public Guid? RoleId { get; set; }
}
