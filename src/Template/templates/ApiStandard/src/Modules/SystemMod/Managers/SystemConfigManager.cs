using System.Text.Json;
using Ater.Common.Options;
using SystemMod.Models.SystemConfigDtos;

namespace SystemMod.Managers;

/// <summary>
/// 系统配置
/// </summary>
public class SystemConfigManager(
    DataAccessContext<SystemConfig> dataContext,
    ILogger<SystemConfigManager> logger,
    IConfiguration configuration,
    CacheService cache
) : ManagerBase<SystemConfig>(dataContext, logger)
{
    private readonly IConfiguration _configuration = configuration;
    private readonly CacheService _cache = cache;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(SystemConfigAddDto dto)
    {
        SystemConfig entity = dto.MapTo<SystemConfigAddDto, SystemConfig>();
        // other required props
        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(SystemConfig entity, SystemConfigUpdateDto dto)
    {
        entity.Merge(dto);
        if (entity.IsSystem)
        {
            dto.Key = null;
            dto.GroupName = null;
        }
        return await UpdateAsync(entity);
    }

    public async Task<PageList<SystemConfigItemDto>> ToPageAsync(SystemConfigFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(
                filter.Key,
                q => q.Key.Contains(filter.Key!, StringComparison.CurrentCultureIgnoreCase)
            )
            .WhereNotNull(filter.GroupName, q => q.GroupName == filter.GroupName);

        return await ToPageAsync<SystemConfigFilterDto, SystemConfigItemDto>(filter);
    }

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
        IQueryable<SystemConfig> query = DbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
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
}
