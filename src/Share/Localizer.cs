using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Resources;

namespace Share;

/// <summary>
/// 本地化资源
/// </summary>
public partial class Localizer(IStringLocalizer<Localizer> localizer)
{
    private static readonly ResourceManager NativeEnglish = new("Share.NativeEnglish", typeof(Localizer).Assembly);
    private static readonly ResourceManager NativeChinese = new("Share.NativeChinese", typeof(Localizer).Assembly);

    public string Get(string key, params object[] arguments)
    {
        var localized = localizer[key, arguments];
        if (!localized.ResourceNotFound)
            return localized.Value;

        var resources = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "zh"
            ? NativeChinese : NativeEnglish;
        var template = resources.GetString(key, CultureInfo.InvariantCulture) ?? key;
        return string.Format(CultureInfo.CurrentCulture, template, arguments);
    }
}
