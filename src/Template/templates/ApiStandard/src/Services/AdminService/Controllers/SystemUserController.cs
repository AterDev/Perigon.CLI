using Ater.Common.Models;
using Ater.Common.Options;
using Ater.Web.Convention.Services;
using CommonMod.Managers;
using Microsoft.AspNetCore.RateLimiting;
using Share.Models.Auth;
using SystemMod.Models.SystemUserDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统用户
/// </summary>
public class SystemUserController(
    Share.Localizer localizer,
    UserContext user,
    ILogger<SystemUserController> logger,
    SystemUserManager manager,
    SystemConfigManager systemConfig,
    CacheService cache,
    IConfiguration config,
    EmailManager emailManager,
    SystemLogService logService,
    SystemRoleManager roleManager
) : RestControllerBase<SystemUserManager>(localizer, manager, user, logger)
{
    private readonly SystemConfigManager _systemConfig = systemConfig;
    private readonly CacheService _cache = cache;
    private readonly IConfiguration _config = config;
    private readonly EmailManager _emailManager = emailManager;
    private readonly SystemLogService _logService = logService;
    private readonly SystemRoleManager _roleManager = roleManager;

    /// <summary>
    /// 登录时，发送邮箱验证码 ✅
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("verifyCode")]
    [AllowAnonymous]
    public async Task<ActionResult> SendVerifyCodeAsync(string email)
    {
        if (!await _manager.IsExistAsync(email))
        {
            return NotFound("不存在的邮箱账号");
        }
        var captcha = SystemUserManager.GetCaptcha();
        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await _cache.GetValueAsync<string>(key) != null)
        {
            return Conflict("验证码已发送!");
        }

        // 使用 smtp，可替换成其他
        await _emailManager.SendLoginVerifyAsync(email, captcha);
        // 缓存，默认5分钟过期
        await _cache.SetValueAsync(key, captcha, 60 * 5);
        return Ok();
    }

    /// <summary>
    /// 获取图形验证码 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("captcha")]
    [EnableRateLimiting(WebConst.Limited)]
    [AllowAnonymous]
    public ActionResult GetCaptchaImage()
    {
        return File(_manager.GetCaptchaImage(4), "image/png");
    }

    /// <summary>
    /// 登录获取Token ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResult>> LoginAsync(SystemLoginDto dto)
    {
        dto.Password = dto.Password.Trim();
        // 查询用户
        SystemUser? user = await _manager.FindByUserNameAsync(dto.UserName);
        if (user == null)
        {
            return NotFound("不存在该用户");
        }

        var loginPolicy = await _systemConfig.GetLoginSecurityPolicyAsync();

        if (await _manager.ValidateLoginAsync(dto, user, loginPolicy))
        {
            user.LastLoginTime = DateTimeOffset.UtcNow;

            // 菜单和权限信息
            var menus = new List<SystemMenu>();
            var permissionGroups = new List<SystemPermissionGroup>();
            if (user.SystemRoles != null)
            {
                menus = await _roleManager.GetSystemMenusAsync([.. user.SystemRoles]);
                permissionGroups = await _roleManager.GetPermissionGroupsAsync(
                    [.. user.SystemRoles]
                );
            }

            AccessTokenDto jwtToken = _manager.GenerateJwtToken(user);

            // 缓存登录状态
            var client = Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
            if (loginPolicy.SessionLevel == SessionLevel.OnlyOne)
            {
                client = WebConst.AllPlatform;
            }
            var key = user.GetUniqueKey(WebConst.LoginCachePrefix, client);
            // 若会话过期时间为0，则使用jwt过期时间

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
                "登录",
                UserActionType.Login,
                "登录成功",
                user.UserName,
                user.Id
            );
            return new AuthResult
            {
                Id = user.Id,
                Username = user.UserName,
                Menus = menus,
                Roles =
                    user.SystemRoles?.Select(r => r.NameValue).ToArray() ?? [WebConst.AdminUser],
                PermissionGroups = permissionGroups,
                AccessToken = jwtToken.AccessToken,
                ExpiresIn = jwtToken.ExpiresIn,
                RefreshToken = jwtToken.RefreshToken,
            };
        }
        else
        {
            await _logService.NewLog(
                "登录",
                UserActionType.Login,
                "登录失败:" + _manager.ErrorStatus,
                user.UserName,
                user.Id
            );
            return Problem(errorCode: _manager.ErrorStatus);
        }
    }

    /// <summary>
    /// 刷新 token
    /// </summary>
    /// <param name="refreshToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="Exception"></exception>
    [HttpGet("refresh_token")]
    public async Task<ActionResult<AccessTokenDto>> RefreshTokenAsync(string refreshToken)
    {
        var userId = await _cache.GetValueAsync<string>(refreshToken);
        if (userId == null || userId != _user.UserId.ToString())
        {
            return NotFound(Localizer.NotFoundResource);
        }
        SystemUser? user = await _manager.FindAsync(Guid.Parse(userId));
        if (user == null)
        {
            return Forbid(Localizer.NotFoundUser);
        }
        AccessTokenDto jwtToken = _manager.GenerateJwtToken(user);
        // 更新缓存
        var loginPolicy = await _systemConfig.GetLoginSecurityPolicyAsync();

        var client = Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
        if (loginPolicy.SessionLevel == SessionLevel.OnlyOne)
        {
            client = WebConst.AllPlatform;
        }
        var key = user.GetUniqueKey(WebConst.LoginCachePrefix, client);

        await _cache.SetValueAsync(refreshToken, user.Id.ToString(), jwtToken.RefreshExpiresIn);
        await _cache.SetValueAsync(key, jwtToken.AccessToken, jwtToken.ExpiresIn);
        return jwtToken;
    }

    /// <summary>
    /// 退出 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPut("logout/{id}")]
    public async Task<ActionResult<bool>> LogoutAsync([FromRoute] Guid id)
    {
        if (await _manager.ExistAsync(id))
        {
            // 清除缓存状态
            await _cache.RemoveAsync(WebConst.LoginCachePrefix + id.ToString());
            return Ok();
        }
        return NotFound();
    }

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<PageList<SystemUserItemDto>>> FilterAsync(
        SystemUserFilterDto filter
    )
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<Guid?>> AddAsync(SystemUserAddDto dto)
    {
        var id = await _manager.AddAsync(dto);
        return id == null ? base.Problem(Localizer.AddFailed) : id;
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, SystemUserUpdateDto dto)
    {
        SystemUser? current = await _manager.GetCurrentAsync(id);
        return current == null
            ? base.NotFound(Localizer.NotFoundResource)
            : await base._manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 修改密码 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPut("changePassword")]
    public async Task<ActionResult<bool>> ChangePassword(string password, string newPassword)
    {
        if (!await _manager.ExistAsync(_user.UserId))
        {
            return NotFound("");
        }
        SystemUser? user = await _manager.GetCurrentAsync(_user.UserId);
        return !HashCrypto.Validate(password, user!.PasswordSalt, user.PasswordHash)
            ? Problem("当前密码不正确")
            : await _manager.ChangePasswordAsync(user, newPassword);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<SystemUserDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = _user.IsRole(WebConst.SuperAdmin)
            ? await _manager.FindAsync<SystemUserDetailDto>(u => u.Id == id)
            : await _manager.FindAsync<SystemUserDetailDto>(u => u.Id == _user.UserId);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        SystemUser? entity = await _manager.GetCurrentAsync(id);
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}
