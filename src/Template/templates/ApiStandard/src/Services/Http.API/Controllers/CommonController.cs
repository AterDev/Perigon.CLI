namespace Http.API.Controllers;

/// <summary>
/// 开发调试接口
/// </summary>
[Route("[controller]")]
public class CommonController(Localizer localizer) : RestControllerBase(localizer)
{
    [HttpGet("localizer")]
    public ActionResult CheckLocalizer()
    {
        return Problem(ErrorKeys.ConflictResource);
    }

    [HttpGet("enums")]
    public Dictionary<string, List<EnumDictionary>> GetEnumDictionary()
    {
        var enums = EnumHelper.GetAllEnumInfo();

        enums
            .Values.ToList()
            .ForEach(v =>
            {
                v.ForEach(e =>
                {
                    e.Description = _localizer?.Get(e.Description) ?? e.Description;
                });
            });

        return enums;
    }
}
