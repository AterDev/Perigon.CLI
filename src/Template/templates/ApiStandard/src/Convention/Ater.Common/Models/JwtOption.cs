namespace Ater.Common.Models;
public class JwtOption
{
    public required string ValidAudiences { get; set; }
    public required string ValidIssuer { get; set; }
    public required string Sign { get; set; }

    /// <summary>
    /// 过期时间:小时
    /// </summary>
    public int Expired { get; set; } = 24;
}
