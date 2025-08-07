using EntityFramework.DBProvider;
using OrderMod.Models.OrderDtos;

namespace OrderMod.Managers;

/// <summary>
/// 订单
/// </summary>
public class OrderManager(
    DefaultDbContext dbContext,
    ILogger<OrderManager> logger,
    UserContext userContext,
    ProductManager productManager
) : ManagerBase<DefaultDbContext, Order>(dbContext, logger)
{
    private readonly ProductManager _productManager = productManager;
    private readonly UserContext _userContext = userContext;

    /// <summary>
    /// 创建待添加实体
    /// </summary>
    /// <returns></returns>
    public async Task<Guid?> AddAsync(OrderAddDto dto)
    {
        Product? product = await _productManager.GetCurrentAsync(dto.ProductId);
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
            .WhereNotNull(filter.UserId, q => q.User!.Id == filter.UserId)
            .WhereNotNull(filter.Status, q => q.Status == filter.Status);

        return await ToPageAsync<OrderFilterDto, OrderItemDto>(filter);
    }

    /*/// <summary>
    /// 异步通知 支付结果
    /// </summary>
    public async Task<bool> PayResult(AsyncNotifyModel model)
    {
        return false;
    }*/

    /// <summary>
    /// 当前用户所拥有的对象
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public async Task<Order?> GetOwnedAsync(Guid id)
    {
        IQueryable<Order> query = _dbSet.Where(q => q.Id == id);
        // 获取用户所属的对象
        // query = query.Where(q => q.User.Id == _userContext.UserId);
        return await query.FirstOrDefaultAsync();
    }
}
