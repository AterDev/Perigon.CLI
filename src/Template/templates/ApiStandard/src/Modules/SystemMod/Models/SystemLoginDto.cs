namespace SystemMod.Models;

/// <summary>
/// 登录
/// </summary>
public class SystemLoginDto
{
    [MaxLength(50)]
    public string Email { get; set; } = default!;
    [MaxLength(60)]
    [MinLength(6)]
    public string Password { get; set; } = default!;
    /// <summary>
    /// 验证码
    /// </summary>
    [MaxLength(50)]
    public string? VerifyCode { get; set; }
}
