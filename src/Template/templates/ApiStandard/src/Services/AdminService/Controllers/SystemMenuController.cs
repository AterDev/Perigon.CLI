using Ater.AspNetCore.Models;
using SystemMod.Models.SystemMenuDtos;

namespace AdminService.Controllers;

/// <summary>
/// 系统菜单
/// </summary>
/// <see cref="SystemMenuManager"/>
[Authorize(WebConst.SuperAdmin)]
public class SystemMenuController(
    Localizer localizer,
    IUserContext user,
    ILogger<SystemMenuController> logger,
    IWebHostEnvironment env,
    SystemMenuManager manager
) : RestControllerBase<SystemMenuManager>(localizer, manager, user, logger)
{
    private readonly IWebHostEnvironment _env = env;

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<SystemMenu>>> FilterAsync(SystemMenuFilterDto filter)
    {
        return await _manager.FilterAsync(filter);
    }

    /// <summary>
    /// 菜单同步 ✅
    /// </summary>
    /// <param name="token"></param>
    /// <param name="menus"></param>
    /// <returns></returns>
    [ApiExplorerSettings(IgnoreApi = true)]
    [HttpPost("sync/{token}")]
    [AllowAnonymous]
    public async Task<ActionResult<bool>> SyncSystemMenus(
        string token,
        List<SystemMenuSyncDto> menus
    )
    {
        if (_env.IsProduction())
        {
            return Forbid();
        }
        // 不经过jwt验证，定义自己的key用来开发时同步菜单
        if (token != "MyProjectNameDefaultKey")
        {
            return Forbid();
        }
        if (menus != null && menus.Count != 0)
        {
            return await _manager.SyncSystemMenusAsync(menus);
        }
        return false;
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(SystemMenuAddDto dto)
    {
        if (dto.ParentId != null)
        {
            if (!await _manager.ExistAsync(dto.ParentId.Value))
            {
                return NotFound(Localizer.NotFoundResource);
            }
        }
        SystemMenu entity = dto.MapTo<SystemMenu>();
        if (dto.ParentId != null)
        {
            entity.ParentId = dto.ParentId.Value;
        }
        if (await _manager.UpsertAsync(entity))
        {
            return entity.Id;
        }
        else
        {
            return Problem(Localizer.AddFailed);
        }
    }

    /// <summary>
    /// 更新 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool?>> UpdateAsync([FromRoute] Guid id, SystemMenuUpdateDto dto)
    {
        SystemMenu? current = await _manager.GetCurrentAsync(id);
        if (current == null)
        {
            return NotFound(Localizer.NotFoundResource);
        }
        ;
        current.Merge(dto);
        return await _manager.UpsertAsync(current);
    }

    /// <summary>
    /// ⚠删除 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    // [ApiExplorerSettings(IgnoreApi = true)]
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool?>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        SystemMenu? entity = await _manager.GetOwnedAsync(id);
        if (entity == null)
        {
            return NotFound();
        }
        ;
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}
