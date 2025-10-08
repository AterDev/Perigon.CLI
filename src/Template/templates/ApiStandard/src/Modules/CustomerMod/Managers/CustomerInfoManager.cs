using CustomerMod.Models.CustomerInfoDtos;

namespace CustomerMod.Managers;

/// <summary>
/// 客户信息
/// </summary>
public class CustomerInfoManager(
    DefaultDbContext dbContext,
    ILogger<CustomerInfoManager> logger)
    : ManagerBase<DefaultDbContext, CustomerInfo>(dbContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(CustomerInfoAddDto dto, Guid userId)
    {
        var entity = dto.MapTo<CustomerInfo>();
        entity.RealName = dto.Name;

        var consult = await _dbContext
            .SystemUsers.Where(q => q.Id == dto.ConsultantId)
            .FirstOrDefaultAsync();

        entity.CreatedUserId = userId;
        entity.ManagerId = consult!.Id;

        return await AddAsync(entity) ? entity.Id : null;
    }

    public async Task<bool> UpdateAsync(CustomerInfo entity, CustomerInfoUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<CustomerInfoItemDto>> ToPageAsync(CustomerInfoFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(
                filter.SearchKey,
                q => q.Name == filter.SearchKey || q.Numbering == filter.SearchKey
            )
            .WhereNotNull(filter.ContactInfo, q => q.ContactInfo == filter.ContactInfo)
            .WhereNotNull(filter.CustomerType, q => q.CustomerType == filter.CustomerType)
            .WhereNotNull(filter.FollowUpStatus, q => q.FollowUpStatus == filter.FollowUpStatus)
            .WhereNotNull(filter.GenderType, q => q.GenderType == filter.GenderType);

        return await ToPageAsync<CustomerInfoFilterDto, CustomerInfoItemDto>(filter);
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomerInfoDetailDto?> GetDetailAsync(Guid id)
    {
        return await Queryable
            .Where(q => q.Id == id)
            .ProjectTo<CustomerInfoDetailDto>()
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// 是否唯一
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsConflictAsync(string name, string contactInfo)
    {
        // 自定义唯一性验证参数和逻辑
        return await _dbSet.AnyAsync(q =>
            q.Name.Equals(name) && q.ContactInfo!.Equals(contactInfo)
        );
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<CustomerInfo?> GetOwnedAsync(Guid id)
    {
        var query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }
}
