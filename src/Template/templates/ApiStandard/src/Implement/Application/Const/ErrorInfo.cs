namespace Application.Const;
/// <summary>
/// 错误信息
/// </summary>
public class ErrorInfo
{
    /// <summary>
    /// 默认语言字典
    /// </summary>
    public static Dictionary<int, string> Map { get; } = new Dictionary<int, string>
        {
            { 500001, "密码错误次数过多，账号已锁定" },
            { 500002, "验证码错误" },
            { 500003, "验证码已过期" },
            { 500004, "密码已过期，请修改密码" },
            { 500005, "用户名或密码错误"}
        };

    /// <summary>
    /// 错误信息英文
    /// </summary>
    public static Dictionary<int, string> MapEn { get; } = new Dictionary<int, string>
        {
            { 500001, "Too many password attempts, account locked" },
            { 500002, "Invalid verification code" },
            { 500003, "Verification code expired" },
            { 500004, "Password expired, please change your password" },
            { 500005, "Invalid username or password"}
        };

    public static string Get(int code, string language = "cn")
    {
        if (language == "en")
        {
            return MapEn.TryGetValue(code, out var msg) ? msg : "Unknown error";
        }
        else
        {
            return Map.TryGetValue(code, out var msg) ? msg : "未知错误";
        }
    }
}
