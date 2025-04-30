using CustomerMod.Models.CustomerInfoDtos;
using Share;
using Share.Implement;
namespace CustomerMod.Controllers.AdminControllers;

/// <summary>
/// 客户信息
/// </summary>
/// <see cref="Managers.CustomerInfoManager"/>
public class CustomerInfoController(
    Localizer localizer,
    UserContext user,
    ILogger<CustomerInfoController> logger,
    CustomerInfoManager manager
    ) : AdminControllerBase<CustomerInfoManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<CustomerInfoItemDto>>> FilterAsync(CustomerInfoFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(CustomerInfoAddDto dto)
    {
        if (await _manager.IsConflictAsync(dto.Name, dto.ContactInfo))
        {
            return Conflict(ErrorKeys.ConflictResource);
        }
        var id = await _manager.AddAsync(dto);
        return id == null ? Problem(ErrorKeys.AddFailed) : id;
    }

    /// <summary>
    /// 部分更新
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, CustomerInfoUpdateDto dto)
    {
        var current = await _manager.GetCurrentAsync(id);
        if (current == null) { return NotFound("不存在的资源"); }
        ;
        return await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerInfoDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetDetailAsync(id);
        return (res == null) ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        var entity = await _manager.GetCurrentAsync(id);
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}