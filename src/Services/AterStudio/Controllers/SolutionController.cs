using Share.Models.CommandDtos;

namespace AterStudio.Controllers;

/// <summary>
/// 功能模块
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SolutionController(
    Localizer localizer,
    SolutionManager manager,
    IProjectContext projectContext,
    ILogger<SolutionController> logger
) : BaseController<SolutionManager>(localizer, manager, projectContext, logger)
{
    /// <summary>
    /// 创建新解决方案
    /// </summary>
    /// <returns></returns>
    [HttpPost("newSolution")]
    public async Task<ActionResult<bool>> CreateNewSolution(CreateSolutionDto dto)
    {
        bool res = await _manager.CreateNewSolutionAsync(dto);
        return res ? true : Problem(_manager.ErrorMsg);
    }

    /// <summary>
    /// 获取模块列表
    /// </summary>
    /// <returns></returns>
    [HttpGet("modules")]
    public List<SubProjectInfo> GetModulesInfo()
    {
        return _manager.GetModules();
    }

    /// <summary>
    /// 获取默认模块
    /// </summary>
    /// <returns></returns>
    [HttpGet("defaultModules")]
    public List<ModuleInfo> GetDefaultModules()
    {
        return _manager.GetDefaultModules();
    }

    /// <summary>
    /// 创建模块
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    [HttpPost("createModule")]
    public async Task<ActionResult<bool>> CreateModule(string name)
    {
        bool res = await _manager.CreateModuleAsync(name);
        return res ? true : Problem(_manager.ErrorMsg);
    }
}
