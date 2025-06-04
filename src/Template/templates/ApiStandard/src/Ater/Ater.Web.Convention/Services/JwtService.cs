using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Ater.Web.Convention.Services;

/// <summary>
/// JWT签名
/// </summary>
/// <param name="sign"></param>
/// <param name="audience"></param>
/// <param name="issuer"></param>
public class JwtService(IOptions<JwtOption> options)
{
    public readonly int ExpiredSecond = options.Value.ExpiredSecond;
    public readonly int RefreshExpiredSecond = options.Value.RefreshExpiredSecond;
    private readonly string Sign = options.Value.Sign;
    private readonly string Audience = options.Value.ValidAudiences;
    private readonly string Issuer = options.Value.ValidIssuer;
    public List<Claim>? Claims { get; set; }

    /// <summary>
    /// 生成jwt token
    /// </summary>
    /// <param name="id"></param>
    /// <param name="roles">角色</param>
    /// <returns></returns>
    public string GetToken(string id, string[] roles)
    {
        SymmetricSecurityKey signingKey = new(Encoding.UTF8.GetBytes(Sign));
        SigningCredentials signingCredentials = new(signingKey, SecurityAlgorithms.HmacSha256);
        List<Claim> claims = [new Claim(ClaimTypes.NameIdentifier, id)];
        if (roles.Length != 0)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }
        if (Claims != null && Claims.Count != 0)
        {
            claims.AddRange(Claims);
        }
        JwtSecurityToken jwt = new(Issuer, Audience, claims,
            expires: DateTime.UtcNow.AddSeconds(ExpiredSecond),
            signingCredentials: signingCredentials);
        var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
        return encodedJwt;
    }

    /// <summary>
    /// refresh token
    /// guid+ random string
    /// </summary>
    /// <returns></returns>
    public static string GetRefreshToken()
    {
        var guid = Guid.CreateVersion7().ToString("N");
        return guid + HashCrypto.GetRnd(32, useLow: true);
    }

    /// <summary>
    /// 解析Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static JwtSecurityToken DecodeJwtToken(string token)
    {
        JwtSecurityTokenHandler tokenHandler = new();
        return tokenHandler.ReadJwtToken(token);
    }

    /// <summary>
    /// 获取token内容
    /// </summary>
    /// <param name="token"></param>
    /// <param name="claimType"></param>
    /// <returns></returns>
    public static string GetClaimValue(string token, string claimType)
    {
        JwtSecurityToken jwtToken = DecodeJwtToken(token);
        return jwtToken.Claims.First(c => c.Type == claimType).Value;
    }

}
