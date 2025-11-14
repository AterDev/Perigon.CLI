using Microsoft.Extensions.Localization;

namespace Share;

/// <summary>
/// 本地化资源
/// </summary>
public partial class Localizer(IStringLocalizer<Localizer> localizer)
{
    public string Get(string key, params object[] arguments)
    {
        return localizer[key, arguments];
    }

    // 常用的本地化常量
    public const string UserNotFound = nameof(UserNotFound);
    public const string RoleNotFound = nameof(RoleNotFound);
    public const string InsufficientPermissions = nameof(InsufficientPermissions);
    public const string PasswordComplexityNotMet = nameof(PasswordComplexityNotMet);
    public const string EntityNotFound = nameof(EntityNotFound);
    public const string MenuUpdateFailed = nameof(MenuUpdateFailed);
    public const string PermissionGroupUpdateFailed = nameof(PermissionGroupUpdateFailed);
}
