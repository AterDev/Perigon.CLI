using CommonMod.Managers;

namespace Http.API.Controllers;

/// <summary>
/// CommonController
/// </summary>
public class CommonController(
    Localizer localizer,
    CommonManager manager,
    IUserContext user,
    ILogger<CommonController> logger
) : RestControllerBase<CommonManager>(localizer, manager, user, logger)
{
    /// <summary>
    /// get enum dictionary
    /// </summary>
    /// <returns></returns>
    [HttpGet("enums")]
    public Dictionary<string, List<EnumDictionary>> GetEnumDictionary()
    {
        return _manager.GetEnumDictionary();
    }
}
