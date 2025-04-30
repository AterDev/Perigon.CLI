using Share.Constants;

namespace Share.Models.UserDtos;

/// <summary>
/// 用户注册
/// </summary>
/// <inheritdoc cref="Entity.UserMod.User"/>
public class RegisterDto
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(40)]
    [Required(ErrorMessage = "用户名必填")]
    public required string UserName { get; set; }

    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// 验证码
    /// </summary>
    public string? VerifyCode { get; set; }
    /// <summary>
    /// 密码
    /// </summary>
    [RegularExpression(RegexConst.PasswordRegex, ErrorMessage = "密码不可以是纯数字")]
    //[RegularExpression(Const.StrongPasswordRegex, ErrorMessage = "密码至少一个大写字母、一个小写字母、一个数字和一个特殊字符")]
    [Required(ErrorMessage = "密码必填")]
    public required string Password { get; set; }
}
