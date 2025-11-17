using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Share;

public class UserContext : IUserContext
{
    public Guid UserId { get; init; }

    public Guid? GroupId { get; init; }

    public Guid TenantId { get; init; }

    public string? UserName { get; init; }
    public string? Email { get; set; }

    public bool IsAdmin { get; init; }
    public string? CurrentRole { get; set; }
    public List<string>? Roles { get; set; }
    IReadOnlyList<string>? IUserContext.Roles => Roles;

    public HttpContext? HttpContext { get; set; }

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        HttpContext = httpContextAccessor!.HttpContext;
        if (
            Guid.TryParse(FindClaim(ClaimTypes.NameIdentifier)?.Value, out Guid userId)
            && userId != Guid.Empty
        )
        {
            UserId = userId;
        }
        if (
            Guid.TryParse(FindClaim(ClaimTypes.GroupSid)?.Value, out Guid groupSid)
            && groupSid != Guid.Empty
        )
        {
            GroupId = groupSid;
        }

        if (
            Guid.TryParse(FindClaim(CustomClaimTypes.TenantId)?.Value, out Guid tenantId)
            && tenantId != Guid.Empty
        )
        {
            TenantId = tenantId;
        }

        UserName = FindClaim(ClaimTypes.Name)?.Value;
        Email = FindClaim(ClaimTypes.Email)?.Value;

        CurrentRole = FindClaim(ClaimTypes.Role)?.Value;

        Roles = HttpContext?.User?.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
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
}
