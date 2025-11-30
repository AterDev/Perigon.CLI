using Ater.AspNetCore.Services;
using Entity.CommonMod;
using EntityFramework.AppDbContext;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Share;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; set; }
    public string TenantType { get; set; }
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly CacheService _cache;
    private readonly DefaultDbContext _dbContext;

    public TenantContext(
        IHttpContextAccessor httpContextAccessor,
        CacheService cache,
        DefaultDbContext dbContext
    )
    {
        _httpContextAccessor = httpContextAccessor;
        _cache = cache;
        _dbContext = dbContext;
        if (
            Guid.TryParse(FindClaim(CustomClaimTypes.TenantId)?.Value, out Guid tenantId)
            && tenantId != Guid.Empty
        )
        {
            TenantId = tenantId;
        }

        var tenantType = FindClaim(CustomClaimTypes.TenantType);
        TenantType = tenantType?.Value ?? "Normal";
    }

    public Claim? FindClaim(string claimType)
    {
        return _httpContextAccessor?.HttpContext?.User?.FindFirst(claimType);
    }

    public async Task<string?> GetTenantNameAsync()
    {
        var tenant = await GetTenantAsync();
        return tenant?.Name;
    }

    public async Task<string?> GetDbConnectionStringAsync()
    {
        var tenant = await GetTenantAsync();
        return tenant?.DbConnectionString;
    }

    public async Task<string?> GetAnalysisConnectionStringAsync()
    {
        var tenant = await GetTenantAsync();
        return tenant?.AnalysisConnectionString;
    }

    private async Task<Tenant?> GetTenantAsync()
    {
        var cacheKey = $"{WebConst.TenantId}__{TenantId}";
        var tenant = await _cache.GetOrCreateAsync(
                    cacheKey,
                    async (cancellationToken) =>
                    {
                        return await _dbContext
                            .Tenants
                            .FirstOrDefaultAsync(
                            t => t.TenantId == TenantId,
                            cancellationToken
                        );
                    }
                );
        return tenant;
    }
}
