using Share.Models.GenActionDtos;
using Share.Models.GenStepDtos;
using Share.Services;

namespace AterStudio.Controllers;

/// <summary>
/// 生成操作
/// </summary>
public class GenActionController(
    Localizer localizer,
    IProjectContext project,
    ILogger<GenActionController> logger,
    GenActionManager manager
    ) : BaseController<GenActionManager>(localizer, manager, project, logger)
{
    /// <summary>
    /// 分页数据
    /// </summary>
    /// <param name="filter"></param>
    /// <returns></returns>
    [HttpPost("filter")]
    public async Task<ActionResult<PageList<GenActionItemDto>>> FilterAsync(GenActionFilterDto filter)
    {
        return await _manager.ToPageAsync(filter);
    }

    /// <summary>
    /// 获取操作步骤
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("steps/{id}")]
    public async Task<ActionResult<List<GenStepItemDto>>> GetStepsAsync(Guid id)
    {
        return await _manager.GetStepsAsync(id);
    }

    /// <summary>
    /// 获取模型列表
    /// </summary>
    /// <param name="sourceType"></param>
    /// <returns></returns>
    [HttpGet("modelFile")]
    public List<ModelFileItemDto> GetModelFile(GenSourceType sourceType)
    {
        return _manager.GetModelFile(sourceType);
    }

    /// <summary>
    /// 添加操作步骤
    /// </summary>
    /// <param name="id"></param>
    /// <param name="stepIds"></param>
    /// <returns></returns>
    [HttpPost("steps/{id}")]
    public async Task<ActionResult<bool>> AddStepsAsync(Guid id, List<Guid> stepIds)
    {
        return await _manager.AddStepsAsync(id, stepIds);
    }

    /// <summary>
    /// 新增
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<Guid?>> AddAsync(GenActionAddDto dto)
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
    public async Task<ActionResult<bool>> UpdateAsync([FromRoute] Guid id, GenActionUpdateDto dto)
    {
        var entity = await _manager.GetOwnedAsync(id);
        if (entity == null) { return NotFound(ErrorMsg.NotFoundResource); }
        // 冲突验证
        return await _manager.UpdateAsync(entity, dto);
    }

    /// <summary>
    /// 执行操作
    /// </summary>
    /// <param name="dto">dto</param>
    /// <returns></returns>
    [HttpPost("execute")]
    public async Task<ActionResult<GenActionResultDto>> ExecuteAsync(GenActionRunDto dto)
    {
        var exist = await _manager.ExistAsync(dto.Id);
        if (!exist) { return NotFound(ErrorMsg.NotFoundResource); }
        // return Forbid();
        return await _manager.ExecuteActionAsync(dto);
    }

    /// <summary>
    /// 获取详情
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<GenActionDetailDto?>> GetDetailAsync([FromRoute] Guid id)
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

    /// <summary>
    /// 从本地导入
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    [HttpGet("syncTemplate")]
    public async Task<ActionResult<string>> SyncAsync([FromServices] SolutionService service)
    {
        var res = await service.SyncDataFromLocalAsync();
        if (res.res)
        {
            return Ok(res.message);
        }
        else
        {
            return Problem(res.message ?? "");
        }
    }

    /// <summary>
    /// 保存配置
    /// </summary>
    /// <param name="service"></param>
    /// <returns></returns>
    [HttpGet("saveTemplate")]
    public async Task<ActionResult<bool>> SaveSyncDataAsync([FromServices] SolutionService service)
    {
        return await service.SaveSyncDataLocalAsync();
    }
}