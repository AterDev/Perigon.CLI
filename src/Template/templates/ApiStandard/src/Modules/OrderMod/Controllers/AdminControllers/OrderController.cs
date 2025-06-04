using OrderMod.Models.OrderDtos;
using Share.Implement;
namespace OrderMod.Controllers.AdminControllers;

/// <summary>
/// 订单
/// </summary>
/// <see cref="Managers.OrderManager"/>
public class OrderController(
    Localizer localizer,
    UserContext user,
    ILogger<OrderController> logger,
    OrderManager manager
        ) : AdminControllerBase<OrderManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<OrderItemDto>>> FilterAsync(OrderFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 更新订单状态 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, OrderUpdateDto dto)
    {
        Order? current = await _manager.GetCurrentAsync(id);
        if (current == null)
        {
            return NotFound(ErrorKeys.NotFoundResource);
        }
        ;
        return await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<OrderDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
    }

}