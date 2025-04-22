using Framework.Web.Convention;
using Microsoft.Extensions.Localization;

namespace Application;
/// <summary>
/// 本地化资源
/// </summary>
public class Localizer
{
    private static readonly IStringLocalizer<Localizer> _localizer = WebAppContext.ServiceProvider.GetRequiredService<IStringLocalizer<Localizer>>();

    /// <summary>
    /// 获取本地化内容
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public static string For(string key)
    {
        return _localizer[key];
    }
}
