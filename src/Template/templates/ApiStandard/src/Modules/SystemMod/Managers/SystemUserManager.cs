using System.Security.Claims;
using System.Text.RegularExpressions;
using Ater.AspNetCore.Options;
using Ater.AspNetCore.Toolkit.Helpers;
using EntityFramework.DBProvider;
using Share.Models.Auth;
using SystemMod.Models;
using SystemMod.Models.SystemUserDtos;
using SystemMod.Services;

namespace SystemMod.Managers;

public class SystemUserManager(
    DefaultDbContext dbContext,
    CacheService cache,
    JwtService jwtService,
    SystemConfigManager systemConfig,
    SystemLogService logService,
    ILogger<SystemUserManager> logger,
    IUserContext userContext,
    Localizer localizer,
    SystemUserRoleManager userRoleManager
) : ManagerBase<DefaultDbContext, SystemUser>(dbContext, logger)
{
    private readonly SystemConfigManager _systemConfig = systemConfig;
    private readonly CacheService _cache = cache;
    private readonly SystemLogService _logService = logService;
    private readonly IUserContext _userContext = userContext;
    private readonly Localizer _localizer = localizer;
    private readonly SystemUserRoleManager _userRoleManager = userRoleManager;

    /// <summary>
    /// 获取验证码
    /// 也可自己实现图片验证码
    /// </summary>
    /// <param name="length">验证码长度</param>
    /// <returns></returns>
    public static string GetCaptcha(int length = 6)
    {
        return HashCrypto.GetRnd(length);
    }

    /// <summary>
    /// 获取图形验证码
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public byte[] GetCaptchaImage(int length = 4)
    {
        var code = GetCaptcha(length);
        var width = length * 20;
        return ImageHelper.GenerateImageCaptcha(code, width);
    }

    /// <summary>
    /// 登录安全策略验证
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="user"></param>
    /// <param name="loginPolicy"></param>
    /// <returns></returns>
    public async Task ValidateLoginAsync(
        SystemLoginDto dto,
        SystemUser user,
        LoginSecurityPolicyOption loginPolicy
    )
    {
        if (loginPolicy == null || !loginPolicy.IsEnable)
        {
            return;
        }
        // 刷新锁定状态
        var lastLoginTime = user.LastLoginTime?.ToLocalTime() ?? DateTimeOffset.Now;
        if ((DateTimeOffset.Now - lastLoginTime).Days >= 1)
        {
            user.RetryCount = 0;
            if (user.LockoutEnabled)
            {
                user.LockoutEnabled = false;
            }
        }

        user.LastLoginTime = DateTimeOffset.UtcNow;

        // 锁定状态
        if (user.LockoutEnabled || user.RetryCount >= loginPolicy.LoginRetry)
        {
            user.LockoutEnabled = true;
            throw new BusinessException(Localizer.LockAccountForManyTimes);
        }

        // 验证码处理
        if (loginPolicy.IsNeedVerifyCode)
        {
            if (dto.VerifyCode == null)
            {
                user.RetryCount++;
                throw new BusinessException(Localizer.InvalidVerifyCode);
            }
            var key = WebConst.VerifyCodeCachePrefix + user.Email;
            var code = await _cache.GetValueAsync<string>(key);
            if (code == null)
            {
                user.RetryCount++;
                throw new BusinessException(Localizer.VerifyCodeExpired);
            }
            if (!code.Equals(dto.VerifyCode))
            {
                await _cache.RemoveAsync(key);
                user.RetryCount++;
                throw new BusinessException(Localizer.InvalidVerifyCode);
            }
        }

        // 密码过期时间
        if ((DateTimeOffset.UtcNow - user.LastPwdEditTime).TotalDays > loginPolicy.PasswordExpired)
        {
            user.RetryCount++;
            throw new BusinessException(Localizer.PasswordExpired);
        }

        if (!HashCrypto.Validate(dto.Password, user.PasswordSalt, user.PasswordHash))
        {
            user.RetryCount++;
            throw new BusinessException(Localizer.PasswordInvalid);
        }
    }

    /// <summary>
    /// 生成jwtToken
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public AccessTokenDto GenerateJwtToken(SystemUser user)
    {
        // 加载关联数据
        var roles = user.SystemRoles?.Select(r => r.NameValue)?.ToList() ?? [WebConst.AdminUser];

        // 添加管理员用户标识
        if (!roles.Contains(WebConst.AdminUser))
        {
            roles.Add(WebConst.AdminUser);
        }
        jwtService.Claims = [new(ClaimTypes.Name, user.UserName)];
        var token = jwtService.GetToken(user.Id.ToString(), [.. roles]);

        return new AccessTokenDto
        {
            AccessToken = token,
            ExpiresIn = jwtService.ExpiredSecond,
            RefreshToken = JwtService.GetRefreshToken(),
            RefreshExpiresIn = jwtService.RefreshExpiredSecond,
        };
    }

    /// <summary>
    /// 更新密码
    /// </summary>
    /// <param name="user"></param>
    /// <param name="newPassword"></param>
    /// <returns></returns>
    public async Task<bool> ChangePasswordAsync(SystemUser user, string newPassword)
    {
        user.PasswordSalt = HashCrypto.BuildSalt();
        user.PasswordHash = HashCrypto.GeneratePwd(newPassword, user.PasswordSalt);
        _dbSet.Update(user);
        return await SaveChangesAsync() > 0;
    }

    public async Task<PageList<SystemUserItemDto>> ToPageAsync(SystemUserFilterDto filter)
    {
        Queryable = Queryable.WhereNotNull(
            filter.UserName,
            q =>
                q.UserName == filter.UserName
                || q.PhoneNumber == filter.UserName
                || q.Email == filter.UserName
        );

        if (filter.RoleId != null)
        {
            var role = await _dbContext.SystemRoles.FindAsync(filter.RoleId);
            if (role != null)
            {
                Queryable = Queryable.Where(q => q.SystemRoles.Contains(role));
            }
        }

        if (filter.RoleId != null)
        {
            Queryable = Queryable.Where(q => q.SystemRoles.Any(r => r.Id == filter.RoleId));
        }
        return await ToPageAsync<SystemUserFilterDto, SystemUserItemDto>(filter);
    }

    /// <summary>
    /// 是否存在
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<bool> IsExistAsync(string email)
    {
        return await Queryable.AnyAsync(q => q.Email == email);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemUser?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemUser> query = _dbSet.Where(q => q.Id == id);
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 删除实体
    /// </summary>
    /// <param name="ids"></param>
    /// <param name="softDelete"></param>
    /// <returns></returns>
    public async Task DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        await base.DeleteAsync(ids, softDelete);
    }

    /// <summary>
    /// 验证密码复杂度
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    public async Task<bool> ValidatePasswordAsync(string password)
    {
        var loginPolicy = await _systemConfig.GetLoginSecurityPolicyAsync();
        // 密码复杂度校验
        var pwdReg = loginPolicy.PasswordLevel switch
        {
            PasswordLevel.Simple => "",
            PasswordLevel.Normal => "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).{8,60}$",
            PasswordLevel.Strict => "^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[\\W_]).{8,}$",
            _ => "^.{6,16}$",
        };
        return Regex.IsMatch(password, pwdReg);
    }

    public override async Task<SystemUser?> FindAsync(Guid id)
    {
        return await Queryable
            .Where(q => q.Id == id)
            .Include(q => q.SystemRoles)
            .FirstOrDefaultAsync();
    }

    public async Task<SystemUser?> FindByUserNameAsync(string userName)
    {
        return await _dbSet
            .Where(u => u.UserName.Equals(userName))
            .Include(u => u.SystemRoles)
            .SingleOrDefaultAsync();
    }

    public async Task<AuthResult> LoginAsync(
        SystemLoginDto dto,
        List<SystemMenu> menus,
        List<SystemPermissionGroup> permissionGroups,
        string client
    )
    {
        dto.Password = dto.Password.Trim();
        // 查询用户
        SystemUser? user = await FindByUserNameAsync(dto.UserName);
        if (user == null)
        {
            throw new BusinessException(Localizer.UserNotExists);
        }

        var loginPolicy = await _systemConfig.GetLoginSecurityPolicyAsync();

        try
        {
            await ValidateLoginAsync(dto, user, loginPolicy);
        }
        catch (BusinessException)
        {
            await _logService.NewLog(
                Localizer.LoginAction,
                UserActionType.Login,
                Localizer.LoginFailed,
                user.UserName,
                user.Id
            );
            throw;
        }

        user.LastLoginTime = DateTimeOffset.UtcNow;

        // 菜单和权限信息 使用传入的参数
        // var menus = new List<SystemMenu>();
        // var permissionGroups = new List<SystemPermissionGroup>();
        // if (user.SystemRoles != null)
        // {
        //     menus = await _roleManager.GetSystemMenusAsync([.. user.SystemRoles]);
        //     permissionGroups = await _roleManager.GetPermissionGroupsAsync(
        //         [.. user.SystemRoles]
        //     );
        // }

        AccessTokenDto jwtToken = GenerateJwtToken(user);

        // 缓存登录状态 使用传入的client
        // var client = WebConst.Web; // 默认
        if (loginPolicy.SessionLevel == SessionLevel.OnlyOne)
        {
            client = WebConst.AllPlatform;
        }
        var key = user.GetUniqueKey(WebConst.LoginCachePrefix, client);

        var expiredSeconds =
            loginPolicy.SessionExpiredSeconds == 0
                ? jwtToken.ExpiresIn
                : loginPolicy.SessionExpiredSeconds;

        // 缓存
        await _cache.SetValueAsync(key, jwtToken.AccessToken, expiredSeconds);
        await _cache.SetValueAsync(
            jwtToken.RefreshToken,
            user.Id.ToString(),
            jwtToken.RefreshExpiresIn
        );

        await _logService.NewLog(
            Localizer.LoginAction,
            UserActionType.Login,
            Localizer.LoginSuccess,
            user.UserName,
            user.Id
        );

        return new AuthResult
        {
            Id = user.Id,
            Username = user.UserName,
            Menus = menus,
            Roles = user.SystemRoles?.Select(r => r.NameValue).ToArray() ?? [WebConst.AdminUser],
            PermissionGroups = permissionGroups,
            AccessToken = jwtToken.AccessToken,
            ExpiresIn = jwtToken.ExpiresIn,
            RefreshToken = jwtToken.RefreshToken,
        };
    }

    public async Task<SystemUser> AddAsync(SystemUserAddDto dto, List<SystemRole>? roles)
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            SystemUser entity = dto.MapTo<SystemUser>();
            // 密码处理
            entity.PasswordSalt = HashCrypto.BuildSalt();
            entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);

            await UpsertAsync(entity);

            // 使用中间表管理器处理角色关联，提高性能
            if (roles != null && roles.Count > 0)
            {
                var roleIds = roles.Select(r => r.Id).ToList();
                await _userRoleManager.SetUserRolesAsync(entity.Id, roleIds);
            }

            return entity;
        });
    }

    public async Task<SystemUser> UpdateAsync(
        Guid id,
        SystemUserUpdateDto dto,
        List<SystemRole>? roles
    )
    {
        return await ExecuteInTransactionAsync(async () =>
        {
            var current =
                await FindAsync(id) ?? throw new BusinessException(Localizer.UserNotFound);

            // 权限验证，利用 IUserContext
            if (!CanUserModify(current))
            {
                throw new BusinessException(
                    Localizer.InsufficientPermissions,
                    StatusCodes.Status403Forbidden
                );
            }

            current.Merge(dto);
            if (dto.Password != null)
            {
                if (!await ValidatePasswordAsync(dto.Password))
                {
                    throw new BusinessException(
                        Localizer.PasswordComplexityNotMet,
                        StatusCodes.Status400BadRequest
                    );
                }
                current.PasswordSalt = HashCrypto.BuildSalt();
                current.PasswordHash = HashCrypto.GeneratePwd(dto.Password, current.PasswordSalt);
            }

            await UpsertAsync(current);

            // 使用中间表管理器处理角色关联，提高性能
            if (roles != null)
            {
                var roleIds = roles.Select(r => r.Id).ToList();
                await _userRoleManager.SetUserRolesAsync(current.Id, roleIds);
            }

            return current;
        });
    }

    /// <summary>
    /// 验证用户是否可以修改
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    private bool CanUserModify(SystemUser user)
    {
        // 超级管理员可以修改所有用户，普通用户只能修改自己
        return _userContext.IsRole(WebConst.SuperAdmin) || _userContext.UserId == user.Id;
    }
}
