namespace AterStudio.Controllers;

/// <summary>
///  实体
/// </summary>
public class EntityInfoController(
    Localizer localizer,
    EntityInfoManager manager,
    IProjectContext project,
    ILogger<EntityInfoController> logger
    ) : BaseController<EntityInfoManager>(localizer, manager, project, logger)
{
    /// <summary>
    /// 实体列表
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public ActionResult<List<EntityFile>> List([FromRoute] Guid id)
    {

        return _project.Project == null
            ? NotFound("不存在的项目")
            : _manager.GetEntityFiles(_project.EntityPath!);
    }

    /// <summary>s
    /// 获取dtos
    /// </summary>
    /// <param name="entityFilePath"></param>
    /// <returns></returns>
    [HttpGet("dtos")]
    public ActionResult<List<EntityFile>> GetDtos(string entityFilePath)
    {
        return !System.IO.File.Exists(entityFilePath)
            ? NotFound("不存在的文件") : _manager.GetDtos(entityFilePath);
    }

    /// <summary>
    /// 清理解决方案
    /// </summary>
    /// <returns></returns>
    [HttpDelete]
    public string CleanSolution()
    {
        bool res = _manager.CleanSolution(out string? errorMsg);
        return res ? "清理成功" : errorMsg;

    }

    /// <summary>
    /// 获取文件内容 entity/manager
    /// </summary>
    /// <param name="entityName"></param>
    /// <param name="isManager">是否为manager</param>
    /// <param name="moduleName"></param>
    /// <returns></returns>
    [HttpGet("fileContent")]
    public EntityFile? GetFileContent(string entityName, bool isManager, string? moduleName = null)
    {
        return _manager.GetFileContent(entityName, isManager, moduleName);
    }


    /// <summary>
    /// 更新内容
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("dto")]
    public async Task<bool> UpdateDtoContentAsync(UpdateDtoDto dto)
    {
        return await _manager.UpdateDtoContentAsync(dto.FileName, dto.Content);
    }

    /// <summary>
    /// 代码生成
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost("generate")]
    public async Task<ActionResult<bool>> GenerateAsync(GenerateDto dto)
    {
        if (_project.Project == null)
        {
            return NotFound("项目不存在");
        }
        await _manager.GenerateAsync(dto);
        return true;
    }
}
