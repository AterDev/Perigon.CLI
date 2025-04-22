using Application;

using Entity.OrderMod;

using OrderMod.Managers;
using OrderMod.Models.ProductDtos;
namespace OrderMod.Controllers;

/// <summary>
/// 产品
/// </summary>
/// <see cref="Managers.ProductManager"/>
public class ProductController(
    IUserContext user,
    ILogger<ProductController> logger,
    ProductManager manager
        ) : ClientControllerBase<ProductManager>(manager, user, logger)
{

    /// <summary>
    /// 产品列表 ✅
    /// </summary>
    /// <returns></returns>
    [HttpGet("list")]
    public async Task<ActionResult<List<ProductItemDto>>> FilterAsync()
    {
        return await _manager.ToListAsync<ProductItemDto>();
    }

    /// <summary>
    /// 购买产品 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost("buy/{id}")]
    public async Task<ActionResult<Order>> BuyProductAsync([FromRoute] Guid id)
    {
        Order? res = await _manager.BuyProductAsync(id);
        return res == null ? Problem(_manager.ErrorMsg) : (ActionResult<Order>)res;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<ProductDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
    }
}