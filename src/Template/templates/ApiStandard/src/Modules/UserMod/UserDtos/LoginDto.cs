namespace UserMod.UserDtos;

/// <summary>
/// 登录
/// </summary>
public class LoginDto
{
    [MaxLength(50)]
    public string UserName { get; set; } = default!;

    [MaxLength(60)]
    public string Password { get; set; } = default!;

    /// <summary>
    /// 验证码
    /// </summary>
    [MaxLength(50)]
    public string? VerifyCode { get; set; }
}
