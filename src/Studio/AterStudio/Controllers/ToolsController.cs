using StudioMod.Managers;
using StudioMod.Models;

namespace AterStudio.Controllers;
/// <summary>
/// Tools
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ToolsController(ToolsManager toolsManager) : ControllerBase
{
    private readonly ToolsManager _toolsManager = toolsManager;

    /// <summary>
    /// 转换成类
    /// </summary>
    /// <returns></returns>
    [HttpPost("classModel")]
    public ActionResult<List<string>?> ConvertToClass([FromBody] ConvertDto dto)
    {
        List<string>? res = _toolsManager.ConvertToClass(dto.Content);
        return res == null ? (ActionResult<List<string>?>)Problem("未能转换成功,请输入合法的json") : (ActionResult<List<string>?>)res;
    }

    /// <summary>
    /// 字符串处理
    /// </summary>
    /// <param name="content"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    [HttpGet("string")]
    public ActionResult<Dictionary<string, string>> ConvertString(string content, StringConvertType type)
    {
        Dictionary<string, string> res = _toolsManager.ConvertString(content, type);
        return res;
    }
}
