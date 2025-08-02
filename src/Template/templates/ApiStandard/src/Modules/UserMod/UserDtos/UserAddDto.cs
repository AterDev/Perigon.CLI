using Entity.UserMod;

namespace UserMod.UserDtos;

/// <summary>
/// 用户账户添加时请求结构
/// </summary>
/// <inheritdoc cref="User"/>
public class UserAddDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(40)]
    public required string UserName { get; set; }

    /// <summary>
    /// 用户类型
    /// </summary>
    public UserType UserType { get; set; } = UserType.Normal;
    public string Password { get; set; } = "123456";

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [MaxLength(200)]
    public string? Avatar { get; set; }
}
