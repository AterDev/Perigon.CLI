using EntityFramework.AppDbFactory;
using System.Text.Json;
using SystemMod.Models.SystemConfigDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统配置
/// </summary>
public class SystemConfigManager(
    TenantDbFactory dbContextFactory,
    ILogger<SystemConfigManager> logger,
    IUserContext userContext,
    IConfiguration configuration,
    CacheService cache
) : ManagerBase<DefaultDbContext, SystemConfig>(dbContextFactory, userContext, logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly CacheService _cache = cache;

    /// <summary>
    /// 获取枚举信息
    /// </summary>
    /// <returns></returns>
    public async Task<Dictionary<string, List<EnumDictionary>>> GetEnumConfigsAsync()
    {
        // 程序启动时更新缓存
        var res = await _cache.GetValueAsync<Dictionary<string, List<EnumDictionary>>>(
            WebConst.EnumCacheName
        );
        if (res == null || res.Count == 0)
        {
            Dictionary<string, List<EnumDictionary>> data = EnumHelper.GetAllEnumInfo();
            await _cache.SetValueAsync(WebConst.EnumCacheName, data, null);
            return data;
        }
        return res;
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<SystemConfig?> GetOwnedAsync(Guid id)
    {
        IQueryable<SystemConfig> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// 获取登录安全策略
    /// </summary>
    /// <returns></returns>
    public async Task<LoginSecurityPolicyOption> GetLoginSecurityPolicyAsync()
    {
        // 优先级：缓存>配置文件
        var policy = new LoginSecurityPolicyOption();
        var configString = await _cache.GetValueAsync<string>(WebConst.LoginSecurityPolicy);
        if (configString != null)
        {
            policy = JsonSerializer.Deserialize<LoginSecurityPolicyOption>(configString);
        }
        else
        {
            var config = _configuration.GetSection(LoginSecurityPolicyOption.ConfigPath);
            if (config.Exists())
            {
                policy = config.Get<LoginSecurityPolicyOption>();
            }
        }
        return policy ?? new LoginSecurityPolicyOption();
    }

    /// <summary>
    /// Add system config
    /// </summary>
    public async Task<SystemConfig> AddAsync(SystemConfigAddDto dto)
    {
        var entity = dto.MapTo<SystemConfig>();
        await InsertAsync(entity);
        return entity;
    }

    /// <summary>
    /// Edit system config
    /// </summary>
    public async Task<int> EditAsync(Guid id, SystemConfigUpdateDto dto)
    {
        if (await HasPermissionAsync(id))
        {
            return await UpdateAsync(id, dto);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    /// <summary>
    /// Get detail
    /// </summary>
    public async Task<SystemConfigDetailDto?> GetAsync(Guid id)
    {
        return await FindAsync<SystemConfigDetailDto>(c => c.Id == id);
    }

    /// <summary>
    /// Filter (pagination)
    /// </summary>
    public async Task<PageList<SystemConfigItemDto>> FilterAsync(SystemConfigFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(
                filter.Key,
                q => q
                    .Key
                    .Contains(filter.Key!, StringComparison.CurrentCultureIgnoreCase)
            )
            .WhereNotNull(filter.GroupName, q => q.GroupName == filter.GroupName);

        return await PageListAsync<SystemConfigFilterDto, SystemConfigItemDto>(filter);
    }

    /// <summary>
    /// Delete
    /// </summary>
    public async Task<int> DeleteAsync(Guid id)
    {
        if (await HasPermissionAsync(id))
        {
            return await DeleteOrUpdateAsync([id], false);
        }
        throw new BusinessException(Localizer.NoPermission);
    }

    public override Task<bool> HasPermissionAsync(Guid id)
    {
        // allow if exists in tenant
        var query = _dbSet.Where(q => q.Id == id && q.TenantId == _userContext.TenantId);
        return query.AnyAsync();
    }
}
