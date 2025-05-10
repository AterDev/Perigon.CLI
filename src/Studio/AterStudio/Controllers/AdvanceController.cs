namespace AterStudio.Controllers;

/// <summary>
/// 高级功能
/// </summary>
public class AdvanceController(
    Localizer localizer,
    AdvanceManager manager,
    IProjectContext project, ILogger<AdvanceController> logger)
    : BaseController<AdvanceManager>(localizer, manager, project, logger)
{

    /// <summary>
    /// 获取配置
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    [HttpGet("config")]
    public ActionResult<ConfigData> GetConfig(string key)
    {
        ConfigData? data = _manager.GetConfig(key);
        return data != null ? data : Ok();
    }

    /// <summary>
    /// 设置配置
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    [HttpPut("config")]
    public async Task<ActionResult> SetConfigAsync(string key, string value)
    {
        await _manager.SetConfigAsync(key, value);
        return Ok();
    }
}

