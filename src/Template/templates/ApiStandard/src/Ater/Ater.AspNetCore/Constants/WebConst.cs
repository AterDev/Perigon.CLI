namespace Ater.AspNetCore.Constants;

/// <summary>
/// 应用程序常量
/// </summary>
public static class WebConst
{
    public const string ProjectName = "MyProjectName";
    public const string DefaultStateName = "statestore";
    public const string DefaultPubSubName = "pubsub";
    public const string TenantId = "TenantId";

    public const string Default = "Default";
    public const string Limited = "Limited";

    /// <summary>
    /// 超级管理员
    /// </summary>
    public const string SuperAdmin = "SuperAdmin";

    /// <summary>
    /// 管理员 policy
    /// </summary>
    public const string AdminUser = "AdminUser";

    /// <summary>
    /// 普通用户 policy
    /// </summary>
    public const string User = "User";

    /// <summary>
    /// 版本
    /// </summary>
    public const string Version = "Version";

    /// <summary>
    /// 登录安全策略
    /// </summary>
    public const string LoginSecurityPolicy = "LoginSecurityPolicy";

    /// <summary>
    /// 系统配置组名称
    /// </summary>
    public const string SystemGroup = "System";

    /// <summary>
    /// 初始化
    /// </summary>
    public const string IsInit = "IsInit";

    /// <summary>
    /// 用户登录缓存前缀
    /// </summary>
    public const string LoginCachePrefix = "Login_";

    /// <summary>
    /// 验证码缓存前缀
    /// </summary>
    public const string VerifyCodeCachePrefix = "VerifyCode_";

    /// <summary>
    /// 枚举缓存
    /// </summary>
    public const string EnumCacheName = "EnumConfigs";

    /// <summary>
    /// 权限缓存前缀
    /// </summary>
    public const string PermissionCacheName = "Permissions";

    public const string Web = "Web";
    public const string IOS = "IOS";
    public const string Android = "Android";
    public const string Desktop = "Desktop";
    public const string Windows = "Windows";
    public const string Linux = "Linux";
    public const string MacOS = "MacOS";

    public const string ClientHeader = "X-Client-Identifier";
    public const string Authorization = "Authorization";
    public const string AllPlatform = "AllPlatform";
}
