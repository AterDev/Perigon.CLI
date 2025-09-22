namespace Ater.Common.Options;

public class JwtOption
{
    public const string ConfigPath = "Authentication:Jwt";

    public required string ValidAudiences { get; set; }
    public required string ValidIssuer { get; set; }
    public required string Sign { get; set; }

    /// <summary>
    /// 过期时间:秒
    /// 默认2小时
    /// </summary>
    public int ExpiredSecond { get; set; } = 7200;

    /// <summary>
    /// 刷新过期时间:秒
    /// 默认7天
    /// </summary>
    public int RefreshExpiredSecond { get; set; } = 7 * 24 * 3600;
}
