using Entity.UserMod;

namespace UserMod.UserDtos;

/// <summary>
/// 用户账户更新时请求结构
/// </summary>
/// <inheritdoc cref="User"/>
public class UserUpdateDto
{
    public UserType? UserType { get; set; }

    [MaxLength(60)]
    public string? Password { get; set; } = "123456";

    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// 头像url
    /// </summary>
    [MaxLength(200)]
    public string? Avatar { get; set; }
}
