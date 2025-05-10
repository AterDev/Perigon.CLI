using Share.Models.GenStepDtos;
namespace AterStudio.Controllers;

/// <summary>
/// task step
/// </summary>
public class GenStepController(
    Localizer localizer,
    IProjectContext projectContext,
    ILogger<GenStepController> logger,
    GenStepManager manager
    ) : BaseController<GenStepManager>(localizer, manager, projectContext, logger)
{

    /// <summary>
    /// 分页数据 
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<GenStepItemDto>>> FilterAsync(GenStepFilterDto filter)
    {
        filter.ProjectId = _project.ProjectId;
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(GenStepAddDto dto)
    {
        // 冲突验证
        // if(await _manager.IsUniqueAsync(dto.xxx)) { return Conflict(ErrorMsg.ConflictResource); }
        var id = await _manager.CreateNewEntityAsync(dto);
        return id == null ? Problem(ErrorMsg.AddFailed) : id;
    }

    /// <summary>
    /// 更新数据
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPatch("{id}")]
    public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, GenStepUpdateDto dto)
    {
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null) { return NotFound(ErrorMsg.NotFoundResource); }
        // 冲突验证
        return await _manager.UpdateAsync(entity, dto);
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<GenStepDetailDto?>> GetDetailAsync([FromRoute] Guid id)
    {
        var res = await _manager.GetDetailAsync(id);
        return res == null ? NotFound() : res;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<bool>> DeleteAsync([FromRoute] Guid id)
    {
        // 注意删除权限
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null) { return NotFound(); }
        ;
        // return Forbid();
        return await _manager.DeleteAsync(entity, false);
    }
}