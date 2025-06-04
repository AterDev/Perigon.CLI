namespace Share.Models.Auth;
/// <summary>
/// 令牌信息
/// </summary>
public class AccessTokenDto
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    /// <summary>
    /// 过期时间秒
    /// </summary>
    public int ExpiresIn { get; set; }

    public int RefreshExpiresIn { get; set; }
}
