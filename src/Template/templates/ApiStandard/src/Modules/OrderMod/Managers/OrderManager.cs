using EntityFramework.DBProvider;
using OrderMod.Models.OrderDtos;

namespace OrderMod.Managers;

/// <summary>
/// 订单
/// </summary>
public class OrderManager(DefaultDbContext dbContext, ILogger<OrderManager> logger)
    : ManagerBase<DefaultDbContext, Order>(dbContext, logger)
{
    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(OrderAddDto dto)
    {
        var product = await _dbContext.Products.FindAsync(dto.ProductId);
        var order = dto.MapTo<Order>();
        // TODO: create order
        return null;
    }

    public async Task<bool> UpdateAsync(Order entity, OrderUpdateDto dto)
    {
        entity.Merge(dto);
        return await UpdateAsync(entity);
    }

    public async Task<PageList<OrderItemDto>> ToPageAsync(OrderFilterDto filter)
    {
        Queryable = Queryable
            .WhereNotNull(filter.OrderNumber, q => q.OrderNumber == filter.OrderNumber)
            .WhereNotNull(filter.ProductId, q => q.Product.Id == filter.ProductId)
            .WhereNotNull(filter.UserId, q => q.UserId == filter.UserId)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await ToPageAsync<OrderFilterDto, OrderItemDto>(filter);
    }

    /// <summary>
    /// TODO: callback from payment gateway
    /// </summary>
    public async Task<bool> PayResultAsync(object model)
    {
        return await Task.FromResult(true);
    }

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Order?> GetOwnedAsync(Guid id)
    {
        IQueryable<Order> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        return await query.FirstOrDefaultAsync();
    }
}
