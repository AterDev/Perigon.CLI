using System.Security.Claims;

using Microsoft.AspNetCore.Http;

namespace Share;
public class TenantProvider : ITenantProvider
{
    public Guid TenantId { get; set; } = Guid.Empty;

    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantProvider(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        if (Guid.TryParse(FindClaim(WebConst.TenantId)?.Value, out Guid userId) && userId != Guid.Empty)
        {
            TenantId = userId;
        }
    }

    public Claim? FindClaim(string claimType)
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(claimType);
    }
}
