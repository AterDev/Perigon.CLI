using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;

namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController(ILogger<ExternalAuthController> logger) : ControllerBase
{
    /// <summary>
    /// Microsft login
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet("signin-microsoft")]
    public IActionResult SignInMicrosoft(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(
                nameof(GetToken),
                new { type = MicrosoftAccountDefaults.AuthenticationScheme, returnUrl }
            ),
        };
        return Challenge(props, "Microsoft");
    }

    /// <summary>
    /// Google login
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet("signin-google")]
    public IActionResult SignInGoogle(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(
                nameof(GetToken),
                new { type = GoogleDefaults.AuthenticationScheme, returnUrl }
            ),
        };
        return Challenge(props, "Google");
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="type"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet("getToken")]
    public async Task<IActionResult> GetToken(string type, string returnUrl = "/")
    {
        logger.LogInformation("{type} login callback initiated.", type);

        // 验证外部登录信息
        var result = await HttpContext.AuthenticateAsync(
            MicrosoftAccountDefaults.AuthenticationScheme
        );
        if (!result.Succeeded)
        {
            result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        }

        if (!result.Succeeded)
        {
            logger.LogWarning("External authentication failed for type: {type}", type);
            return BadRequest("External authentication failed.");
        }

        // 提取用户信息
        var externalUser = result.Principal;
        var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
        var name = externalUser.FindFirst(ClaimTypes.Name)?.Value;
        // TODO:根据信息进行后续处理
        return Ok(new { email, name });
    }
}
