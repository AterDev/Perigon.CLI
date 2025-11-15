namespace SystemMod.Models.SystemUserDtos;

/// <summary>
/// 系统用户更新时请求结构
/// </summary>
/// <inheritdoc cref="SystemUser"/>
public class SystemUserUpdateDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(30)]
    public string? UserName { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    [MaxLength(60)]
    public string? Password { get; set; }

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(30)]
    public string? RealName { get; set; }

    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(20)]
    [Phone]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [MaxLength(200)]
    public string? Avatar { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType? Sex { get; set; }

    /// <summary>
    /// 角色Id
    /// </summary>
    public List<Guid>? RoleIds { get; set; }
}
