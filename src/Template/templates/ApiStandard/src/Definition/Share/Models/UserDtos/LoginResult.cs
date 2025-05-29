namespace Share.Models.UserDtos;
public class LoginResult
{
    public Guid Id { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = default!;
    public string[] Roles { get; set; } = default!;
    /// <summary>
    /// token
    /// </summary>
    public string AccessToken { get; set; } = default!;
    /// <summary>
    /// 过期时间秒
    /// </summary>
    public int ExpiresIn { get; set; }
    public string RefreshToken { get; set; } = default!;
}
