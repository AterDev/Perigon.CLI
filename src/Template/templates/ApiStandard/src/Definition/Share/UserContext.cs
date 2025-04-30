using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Share;

public class UserContext
{
    /// <summary>
    /// 用户id
    /// </summary>
    public Guid UserId { get; init; }
    /// <summary>
    /// 组织id
    /// </summary>
    public Guid? GroupId { get; init; }
    public string? Username { get; init; }
    public string? Email { get; set; }
    /// <summary>
    /// 是否为管理员
    /// </summary>
    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }

    public HttpContext? HttpContext { get; set; }

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor!.HttpContext;
        if (Guid.TryParse(FindClaim(ClaimTypes.NameIdentifier)?.Value, out Guid userId) && userId != Guid.Empty)
        {
            UserId = userId;
        }
        if (Guid.TryParse(FindClaim(ClaimTypes.GroupSid)?.Value, out Guid groupSid) && groupSid != Guid.Empty)
        {
            GroupId = groupSid;
        }
        Username = FindClaim(ClaimTypes.Name)?.Value;
        Email = FindClaim(ClaimTypes.Email)?.Value;

        CurrentRole = FindClaim(ClaimTypes.Role)?.Value;

        Roles = HttpContext?.User?.FindAll(ClaimTypes.Role)
            .Select(c => c.Value).ToList();
        if (Roles != null)
        {
            IsAdmin = Roles.Any(r => r.Equals(WebConst.AdminUser) || r.Equals(WebConst.SuperAdmin));
        }
    }

    protected Claim? FindClaim(string claimType)
    {
        return HttpContext?.User?.FindFirst(claimType);
    }

    /// <summary>
    /// 判断当前角色
    /// </summary>
    /// <param name="roleName"></param>
    /// <returns></returns>
    public bool IsRole(string roleName)
    {
        return Roles != null && Roles.Any(r => r == roleName);
    }


    /// <summary>
    /// 获取ip地址
    /// </summary>
    /// <returns></returns>
    public string? GetIpAddress()
    {
        HttpRequest? request = HttpContext?.Request;
        return request == null
            ? string.Empty
            : request.Headers.TryGetValue("X-Forwarded-For", out StringValues value)
                ? value.Where(x => x != null).FirstOrDefault()
                : HttpContext!.Connection.RemoteIpAddress?.ToString();
    }
}
