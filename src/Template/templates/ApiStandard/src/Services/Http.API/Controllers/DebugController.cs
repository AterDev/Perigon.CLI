namespace Http.API.Controllers;

/// <summary>
/// 开发调试接口
/// </summary>
[Route("[controller]")]
public class DebugController(Localizer localizer, DebugManager manager)
    : RestControllerBase(localizer)
{

    [HttpGet]
    public void DebugInfo()
    {
        throw new Exception("DebugInfo");
    }

    [HttpGet("localizer")]
    public ActionResult CheckLocalizer()
    {
        return Problem(ErrorKeys.ConflictResource);
    }
}
