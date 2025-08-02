using UserMod.UserDtos;

namespace UserMod.Controllers.AdminControllers;

/// <summary>
/// 用户账户
/// </summary>
/// <see cref="CommonMod.Managers.UserManager"/>
[Authorize(WebConst.AdminUser)]
public class UserController(
    Localizer localizer,
    UserContext user,
    ILogger<UserController> logger,
    UserManager manager
) : AdminControllerBase<UserManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<UserItemDto>>> FilterAsync(UserFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(UserAddDto dto)
    {
        // 判断重复用户名
        if (await _manager.IsUniqueAsync(dto.UserName))
        {
            return Conflict(ErrorKeys.ExistUser);
        }
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
    public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, UserUpdateDto dto)
    {
        var current = await _manager.GetOwnedAsync(id);
        return current == null
            ? NotFound(ErrorKeys.NotFoundResource)
            : await _manager.UpdateAsync(current, dto);
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetDetailAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [NonAction]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        var res = await _manager.GetOwnedAsync(id);
        return res == null
            ? NotFound(ErrorKeys.NotFoundResource)
            : await _manager.DeleteAsync([id], true);
    }
}
