namespace Framework.Common.Models;
public  class JwtOption
{
    public const string ConfigPath = "Authentication:Jwt";

    public required string ValidAudiences { get; set; }
    public required string ValidIssuer { get; set; }
    public required string Sign { get; set; }

    /// <summary>
    /// 过期时间:小时
    /// </summary>
    public int ExpiredSecond { get; set; } = 7200;
}
