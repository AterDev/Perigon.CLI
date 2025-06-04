using OrderMod.Models.ProductDtos;
using Share.Implement;
namespace OrderMod.Controllers.AdminControllers;

/// <summary>
/// 产品
/// </summary>
/// <see cref="Managers.ProductManager"/>
public class ProductController(
    Localizer localizer,
    UserContext user,
    ILogger<ProductController> logger,
    ProductManager manager
        ) : AdminControllerBase<ProductManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<ProductItemDto>>> FilterAsync(ProductFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(ProductAddDto dto)
    {
        var id = await _manager.AddAsync(dto);
        return id == null ? Problem(ErrorKeys.AddFailed) : id;
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, ProductUpdateDto dto)
    {
        Product? current = await _manager.GetCurrentAsync(id);
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
    public async Task<ActionResult<ProductDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<ProductDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// 删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        Product? entity = await _manager.GetCurrentAsync(id);
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}