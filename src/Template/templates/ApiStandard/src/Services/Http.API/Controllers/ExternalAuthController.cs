using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Http.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExternalAuthController : ControllerBase
{
    private readonly IConfiguration _config;
    public ExternalAuthController(IConfiguration config)
    {
        _config = config;
    }

    // 1. 发起微软登录
    [HttpGet("signin-microsoft")]
    public IActionResult SignInMicrosoft(string returnUrl = "/")
    {
        var props = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(MicrosoftCallback), new { returnUrl })
        };
        return Challenge(props, "Microsoft");
    }

    // 2. 登录回调
    [HttpGet("microsoft-callback")]
    public async Task<IActionResult> MicrosoftCallback(string returnUrl = "/")
    {
        // 验证外部登录信息
        var result = await HttpContext.AuthenticateAsync("Microsoft");
        if (!result.Succeeded)
        {
            return BadRequest("External authentication failed");
        }

        // 提取微软用户信息
        var externalUser = result.Principal;
        var email = externalUser.FindFirst(ClaimTypes.Email)?.Value;
        var name = externalUser.FindFirst(ClaimTypes.Name)?.Value;
        // … 你可以在此根据 email 查库，创建或更新本地用户 …

        // 3. 生成自己的 JWT
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_config["Authentication:Jwt:SecretKey"]);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, email!),
                new Claim(ClaimTypes.Name, name!)
                // … 其它自定义 Claim …
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            ),
            Issuer = _config["Authentication:Jwt:Authority"],
            Audience = _config["Authentication:Jwt:Audience"]
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        var jwt = tokenHandler.WriteToken(token);

        // 4. 返回 JWT 给前端
        return Ok(new
        {
            access_token = jwt,
            token_type = "Bearer",
            expires_in = 3600
        });
    }
}
