using Entity.UserMod;
using Microsoft.Extensions.Configuration;
using Share.Models.Auth;
using UserMod.UserDtos;

namespace UserMod.Controllers;

/// <summary>
/// 用户账户
/// </summary>
/// <see cref="CommonMod.Managers.UserManager"/>
public class UserController(
    Share.Localizer localizer,
    UserContext user,
    ILogger<UserController> logger,
    UserManager manager,
    CacheService cache,
    JwtService jwtService,
    EmailManager emailManager,
    IConfiguration config
) : RestControllerBase<UserManager>(localizer, manager, user, logger)
{
    private readonly CacheService _cache = cache;
    private readonly IConfiguration _config = config;

    /// <summary>
    /// 用户注册 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [AllowAnonymous]
    public async Task<ActionResult<User>> RegisterAsync(RegisterDto dto)
    {
        // 判断重复用户名
        if (await _manager.ExistAsync(q => q.UserName.Equals(dto.UserName)))
        {
            return Conflict(Localizer.ExistUser);
        }
        // 根据实际需求自定义验证码逻辑
        if (dto.VerifyCode != null)
        {
            if (dto.Email == null)
            {
                return BadRequest("邮箱不能为空");
            }
            var key = WebConst.VerifyCodeCachePrefix + dto.Email;
            var code = await _cache.GetValueAsync<string>(key);
            if (code == null)
            {
                return BadRequest("验证码已过期");
            }
            if (!code.Equals(dto.VerifyCode))
            {
                return BadRequest("验证码错误");
            }
        }
        return await _manager.RegisterAsync(dto);
    }

    /// <summary>
    /// 注册时，发送邮箱验证码 ✅
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("regVerifyCode")]
    [AllowAnonymous]
    public async Task<ActionResult> SendRegVerifyCodeAsync(string email)
    {
        var captcha = HashCrypto.GetRnd(6);
        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await _cache.GetValueAsync<string>(key) != null)
        {
            return Conflict("验证码已发送!");
        }
        // 使用 smtp，可替换成其他
        await emailManager.SendRegisterVerifyAsync(email, captcha);
        // 缓存，默认5分钟过期
        await _cache.SetValueAsync(key, captcha, 60 * 5);
        return Ok();
    }

    /// <summary>
    /// 登录时，发送邮箱验证码 ✅
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    [HttpPost("loginVerifyCode")]
    [AllowAnonymous]
    public async Task<ActionResult> SendVerifyCodeAsync(string email)
    {
        if (!await _manager.ExistAsync(q => q.Email != null && q.Email.Equals(email)))
        {
            return NotFound("不存在的邮箱账号");
        }
        var captcha = HashCrypto.GetRnd(6);
        var key = WebConst.VerifyCodeCachePrefix + email;
        if (await _cache.GetValueAsync<string>(key) != null)
        {
            return Conflict("验证码已发送!");
        }
        // 使用 smtp，可替换成其他
        await emailManager.SendLoginVerifyAsync(email, captcha);
        // 缓存，默认5分钟过期
        await _cache.SetValueAsync(key, captcha, 60 * 5);

        return Ok();
    }

    /// <summary>
    /// 登录获取Token ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResult>> LoginAsync(LoginDto dto)
    {
        // 查询用户
        User? user = await _manager.FindAsync<User>(u => u.UserName.Equals(dto.UserName));
        if (user == null)
        {
            return NotFound("不存在该用户");
        }

        // 可将 dto.VerifyCode 设置为必填，以强制验证
        if (dto.VerifyCode != null)
        {
            var key = WebConst.VerifyCodeCachePrefix + user.Email;
            var cacheCode = await _cache.GetValueAsync<string>(key);
            if (cacheCode == null)
            {
                return BadRequest("验证码已过期");
            }
            if (!cacheCode.Equals(dto.VerifyCode))
            {
                return BadRequest("验证码错误");
            }
        }

        // 如果有登录安全策略，可以在此处添加验证逻辑
        // var loginPolicy = await _systemConfig.GetLoginSecurityPolicyAsync();

        if (HashCrypto.Validate(dto.Password, user.PasswordSalt, user.PasswordHash))
        {
            var roles = new List<string> { WebConst.User };
            var token = jwtService.GetToken(user.Id.ToString(), [.. roles]);
            var refreshToken = JwtService.GetRefreshToken();

            // 缓存
            await _cache.SetValueAsync(
                WebConst.LoginCachePrefix + user.Id.ToString(),
                true,
                jwtService.ExpiredSecond
            );
            await _cache.SetValueAsync(
                refreshToken,
                user.Id.ToString(),
                jwtService.RefreshExpiredSecond
            );

            return new LoginResult
            {
                Id = user.Id,
                Roles = [.. roles],
                AccessToken = token,
                ExpiresIn = jwtService.ExpiredSecond,
                RefreshToken = refreshToken,
                Username = user.UserName,
            };
        }
        else
        {
            return Problem("用户名或密码错误", title: "");
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
        // 获取用户信息
        var user = await _manager.FindAsync<User>(u => u.Id.ToString().Equals(userId));
        if (user == null)
        {
            return Forbid(Localizer.NotFoundUser);
        }
        var roles = new List<string> { WebConst.User };
        var token = jwtService.GetToken(user.Id.ToString(), [.. roles]);
        var newRefreshToken = JwtService.GetRefreshToken();

        // 缓存
        await _cache.SetValueAsync(
            WebConst.LoginCachePrefix + user.Id.ToString(),
            true,
            jwtService.ExpiredSecond
        );
        await _cache.SetValueAsync(
            newRefreshToken,
            user.Id.ToString(),
            jwtService.RefreshExpiredSecond
        );
        return new AccessTokenDto
        {
            AccessToken = token,
            RefreshToken = newRefreshToken,
            ExpiresIn = jwtService.ExpiredSecond,
            RefreshExpiresIn = jwtService.RefreshExpiredSecond,
        };
    }

    /// <summary>
    /// 退出 ✅
    /// </summary>
    /// <returns></returns>
    [HttpPut("logout")]
    public async Task<ActionResult<bool>> LogoutAsync()
    {
        if (await _manager.ExistAsync(_user.UserId))
        {
            // 清除缓存状态
            await _cache.RemoveAsync(WebConst.LoginCachePrefix + _user.UserId.ToString());
            return Ok();
        }
        return NotFound();
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
        User? user = await _manager.GetCurrentAsync(_user.UserId);
        return !HashCrypto.Validate(password, user!.PasswordSalt, user.PasswordHash)
            ? Problem("当前密码不正确")
            : await _manager.ChangePasswordAsync(user, newPassword);
    }

    /// <summary>
    /// 更新信息：头像 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut]
    public async Task<ActionResult<bool?>> UpdateAsync(UserUpdateDto dto)
    {
        User? current = await _manager.GetCurrentAsync(_user.UserId);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }
        ;
        return await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<bool?>> GetDetailAsync()
    {
        User? res = await _manager.FindAsync(_user.UserId);
        return res == null
            ? base.NotFound(Localizer.NotFoundResource)
            : await base._manager.DeleteAsync([base._user.UserId], true);
    }
}
