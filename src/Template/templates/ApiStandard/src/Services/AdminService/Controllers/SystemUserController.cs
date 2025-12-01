using Ater.AspNetCore.Models;
using Ater.AspNetCore.Options;
using Ater.AspNetCore.Services;
using Microsoft.AspNetCore.RateLimiting;
using Share.Models.Auth;
using SystemMod.Models.SystemUserDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统用户
/// </summary>
public class SystemUserController(
        Localizer localizer,
        SystemConfigManager systemConfig,
        CacheService cache,
        SystemRoleManager roleManager,
        SystemUserManager manager,
        IUserContext user,
        ILogger<SystemUserController> logger

) : RestControllerBase<SystemUserManager>(localizer, manager, user, logger)
{
    private readonly SystemConfigManager _systemConfig = systemConfig;
    private readonly CacheService _cache = cache;
    private readonly SystemRoleManager _roleManager = roleManager;

    /// <summary>
    /// 登录时，发送邮箱验证码 ✅
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("verifyCode")]
    [AllowAnonymous]
    [EnableRateLimiting(WebConst.Limited)]
    public async Task<ActionResult> SendVerifyCodeAsync(string email)
    {
        if (!await _manager.IsExistAsync(email))
        {
            return BadRequest(Localizer.UserNotFound);
        }

        var captcha = SystemUserManager.GetCaptcha();
        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await _cache.GetValueAsync<string>(key) != null)
        {
            return Conflict(Localizer.VerifyCodeAlreadySent);
        }

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
    /// 返回仅包含 token 信息和用户 id/username
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("login")]
    [AllowAnonymous]
    [EnableRateLimiting(WebConst.Limited)]
    public async Task<AccessTokenDto> LoginAsync(SystemLoginDto dto)
    {
        // 获取client
        var client =
            HttpContext.Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;

        var tokenResult = await _manager.LoginAsync(dto, client);
        return tokenResult;
    }

    /// <summary>
    /// 根据用户 id 获取用户角色、菜单与权限组等信息
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("userinfo/{id}")]
    public async Task<ActionResult<UserInfoDto>> GetUserInfoAsync([FromRoute] Guid id)
    {
        // 获取用户
        var user = await _manager.GetSystemUserAsync(id);
        if (user == null)
        {
            return NotFound(Localizer.NotFoundUser);
        }

        var menus = new List<SystemMenu>();
        var permissionGroups = new List<SystemPermissionGroup>();
        if (user.SystemRoles != null)
        {
            menus = await _roleManager.GetSystemMenusAsync([.. user.SystemRoles]);
            permissionGroups = await _roleManager.GetPermissionGroupsAsync([.. user.SystemRoles]);
        }

        return new UserInfoDto
        {
            Id               = user.Id,
            Username         = user.UserName,
            Roles            = user.SystemRoles?.Select(r => r.NameValue).ToArray() ?? [WebConst.AdminUser],
            Menus            = menus,
            PermissionGroups = permissionGroups,
        };
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
        var client      = HttpContext.Request.Headers[WebConst.ClientHeader].FirstOrDefault() ?? WebConst.Web;
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
    [HttpPost("logout/{id}")]
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
    [HttpGet]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<PageList<SystemUserItemDto>>> FilterAsync([FromQuery] SystemUserFilterDto filter)
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
    public async Task<ActionResult<SystemUser>> AddAsync(SystemUserAddDto dto)
    {
        // 角色处理
        List<SystemRole>? roles = null;
        if (dto.RoleIds != null && dto.RoleIds.Count != 0)
        {
            roles = await _roleManager.ListAsync(r => dto
                .RoleIds
                .Contains(r.Id));
        }
        var entity = await _manager.AddAsync(dto, roles);
        return CreatedAtAction(nameof(GetDetailAsync), new { id = entity.Id }, entity);
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult<SystemUser>> UpdateAsync(
        [FromRoute] Guid id,
        SystemUserUpdateDto dto
    )
    {
        // 角色处理
        List<SystemRole>? roles = null;
        if (dto.RoleIds != null)
        {
            roles = await _roleManager.ListAsync(r => dto
                .RoleIds
                .Contains(r.Id));
        }
        var entity = await _manager.UpdateAsync(id, dto, roles);
        return Ok(entity);
    }

    /// <summary>
    /// 修改密码 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPatch("changePassword")]
    public async Task<ActionResult<bool>> ChangePassword(string password, string newPassword)
    {
        if (!await _manager.ExistAsync(_user.UserId))
        {
            return NotFound("");
        }
        SystemUser? user = await _manager.FindAsync(_user.UserId);
        return !HashCrypto.Validate(password, user!.PasswordSalt, user.PasswordHash)
            ? Problem(Localizer.InvalidUserOrPassword)
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
        var res = await _manager.GetAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(WebConst.SuperAdmin)]
    public async Task<ActionResult> DeleteAsync([FromRoute] Guid id)
    {
        await _manager.DeleteAsync([id], false);
        return NoContent();
    }
}
