using Ater.Common.Options;
using Ater.Web.Extension.Helpers;
using EntityFramework.DBProvider;
using Share.Models.Auth;
using System.Security.Claims;
using System.Text.RegularExpressions;
using SystemMod.Models;
using SystemMod.Models.SystemUserDtos;

namespace SystemMod.Managers;

public class SystemUserManager(
    DefaultDbContext dbContext,
    IConfiguration configuration,
    CacheService cache,
    JwtService jwtService,
    SystemConfigManager systemConfig,
    ILogger<SystemUserManager> logger
) : ManagerBase<DefaultDbContext, SystemUser>(dbContext, logger)
{
    private readonly SystemConfigManager _systemConfig = systemConfig;
    private readonly IConfiguration _configuration = configuration;
    private readonly CacheService _cache = cache;

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
    public async Task<bool> ValidateLoginAsync(
        SystemLoginDto dto,
        SystemUser user,
        LoginSecurityPolicyOption loginPolicy
    )
    {
        if (loginPolicy == null || !loginPolicy.IsEnable)
        {
            return true;
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
            ErrorStatus = 500001;
            return false;
        }

        // 验证码处理
        if (loginPolicy.IsNeedVerifyCode)
        {
            if (dto.VerifyCode == null)
            {
                user.RetryCount++;
                ErrorStatus = 500002;
                return false;
            }
            var key = WebConst.VerifyCodeCachePrefix + user.Email;
            var code = await _cache.GetValueAsync<string>(key);
            if (code == null)
            {
                ErrorStatus = 500003;
                user.RetryCount++;
                return false;
            }
            if (!code.Equals(dto.VerifyCode))
            {
                await _cache.RemoveAsync(key);
                ErrorStatus = 500002;
                user.RetryCount++;
                return false;
            }
        }

        // 密码过期时间
        if ((DateTimeOffset.UtcNow - user.LastPwdEditTime).TotalDays > loginPolicy.PasswordExpired)
        {
            ErrorStatus = 500004;
            user.RetryCount++;
            return false;
        }

        if (!HashCrypto.Validate(dto.Password, user.PasswordSalt, user.PasswordHash))
        {
            ErrorStatus = 500005;
            user.RetryCount++;
            return false;
        }
        return true;
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

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemUserAddDto dto)
    {
        SystemUser entity = dto.MapTo<SystemUserAddDto, SystemUser>();
        // 密码处理
        entity.PasswordSalt = HashCrypto.BuildSalt();
        entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);
        // 角色处理
        if (dto.RoleIds != null && dto.RoleIds.Count != 0)
        {
            var roles = _dbContext.SystemRoles.Where(r => dto.RoleIds.Contains(r.Id)).ToList();
            entity.SystemRoles = roles;
        }
        return await AddAsync(entity) ? entity.Id : null;
    }

    /// <summary>
    /// 更新实体
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<bool> UpdateAsync(SystemUser entity, SystemUserUpdateDto dto)
    {
        entity.Merge(dto);
        if (dto.Password != null)
        {
            entity.PasswordSalt = HashCrypto.BuildSalt();
            entity.PasswordHash = HashCrypto.GeneratePwd(dto.Password, entity.PasswordSalt);
        }
        if (dto.RoleIds != null)
        {
            await LoadManyAsync(entity, e => e.SystemRoles);
            var roles = _dbContext.SystemRoles.Where(r => dto.RoleIds.Contains(r.Id)).ToList();
            entity.SystemRoles = roles;
        }
        return await UpdateAsync(entity);
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
    public new async Task<bool?> DeleteAsync(List<Guid> ids, bool softDelete = true)
    {
        return await base.DeleteAsync(ids, softDelete);
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
        if (!Regex.IsMatch(password, pwdReg))
        {
            ErrorMsg = loginPolicy.PasswordLevel switch
            {
                PasswordLevel.Simple => "密码长度6-60位",
                PasswordLevel.Normal => "密码长度8-60位，必须包含大小写字母和数字",
                PasswordLevel.Strict => "密码长度8位以上，必须包含大小写字母、数字和特殊字符",
                _ => "密码长度6-16位",
            };
            return false;
        }
        return true;
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
}
