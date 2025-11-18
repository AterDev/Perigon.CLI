using System.Security.Claims;
using Ater.AspNetCore.Services;
using Entity.CommonMod;
using EntityFramework.AppDbContext;
using Microsoft.AspNetCore.Http;
using Share.Exceptions;

namespace Share;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public string TenantType { get; set; }
    private readonly Tenant _tenant;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor,
        CacheService cache,
        DefaultDbContext dbContext
    )
    {
        _httpContextAccessor = httpContextAccessor;
        if (
            Guid.TryParse(FindClaim(CustomClaimTypes.TenantId)?.Value, out Guid tenantId)
            && tenantId != Guid.Empty
        )
        {
            TenantId = tenantId;
        }

        var tenantType = FindClaim(CustomClaimTypes.TenantType);
        if (tenantType == null)
        {
            throw new BusinessException("WrongTenantType", StatusCodes.Status400BadRequest);
        }
        TenantType = tenantType.Value;

        var cacheKey = $"{WebConst.TenantId}__{TenantId}";
        var tenant =
            cache
                .GetOrCreateAsync(
                    cacheKey,
                    async (cancellationToken) =>
                    {
                        return await dbContext.Tenants.FirstOrDefaultAsync(
                            t => t.TenantId == TenantId,
                            cancellationToken
                        );
                    }
                )
                .Result
            ?? throw new BusinessException("WrongTenantId", StatusCodes.Status400BadRequest);
        _tenant = tenant;
    }

    public Claim? FindClaim(string claimType)
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(claimType);
    }

    public string GetTenantName()
    {
        throw new NotImplementedException();
    }

    public string GetDbConnectionString()
    {
        throw new NotImplementedException();
    }

    public string GetAnalysisConnectionString()
    {
        throw new NotImplementedException();
    }
}
