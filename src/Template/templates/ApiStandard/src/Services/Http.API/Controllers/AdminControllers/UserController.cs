using Framework.Common.Models;
using Framework.Web.Convention;
using Share.Models.UserDtos;
using SharedModule;
using SharedModule.Const;
using SharedModule.Implement;
using SharedModule.Managers;
namespace Http.API.Controllers.AdminControllers;

/// <summary>
/// 用户账户
/// </summary>
/// <see cref="SharedModule.Managers.UserManager"/>
[Authorize(WebConst.AdminUser)]
public class UserController(
    IUserContext user,
    ILogger<UserController> logger,
    UserManager manager
        ) : RestControllerBase<UserManager>(manager, user, logger)
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
            return Conflict(ErrorMsg.ExistUser);
        }
        var id = await _manager.AddAsync(dto);
        return id == null ? Problem(ErrorMsg.AddFailed) : id;
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
        return current == null ? NotFound(ErrorMsg.NotFoundResource) : await _manager.UpdateAsync(current, dto);
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
        return (res == null) ? NotFound() : res;
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
        return res == null ? NotFound(ErrorMsg.NotFoundResource)
            : await _manager.DeleteAsync([id], true);

    }
}