using FileManagerMod.Models.FolderDtos;
using Share.Implement;

namespace FileManagerMod.Controllers.AdminControllers;

/// <summary>
/// 文件夹
/// </summary>
/// <see cref="Managers.FolderManager"/>
public class FolderController(
    Localizer localizer,
    UserContext user,
    ILogger<FolderController> logger,
    FolderManager manager
        ) : AdminControllerBase<FolderManager>(localizer, manager, user, logger)
{

    /// <summary>
    /// 筛选 ✅
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<FolderItemDto>>> FilterAsync(FolderFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增 ✅
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(FolderAddDto dto)
    {
        if (dto.ParentId != null)
        {
            var exist = await _manager.ExistAsync(dto.ParentId.Value);
            if (!exist)
            {
                return NotFound(ErrorKeys.NotFoundResource);
            }
            ;
        }
        var id = await _manager.AddAsync(dto);
        return id == null ? Problem(ErrorKeys.AddFailed) : id;
    }

    /// <summary>
    /// 详情 ✅
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<FolderDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.FindAsync<FolderDetailDto>(d => d.Id == id);
        return (res == null) ? NotFound() : res;
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
        Folder? entity = await _manager.GetCurrentAsync(id);
        // return Forbid();
        return entity == null ? NotFound() : await _manager.DeleteAsync([id], false);
    }
}