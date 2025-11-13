using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Share.Implement;

[ApiExplorerSettings(GroupName = "v1")]
[Authorize(Policy = WebConst.User)]
public class RestControllerBase<TManager>(
    Localizer localizer,
    TManager manager,
    IUserContext user,
    ILogger logger
) : RestControllerBase(localizer)
    where TManager : class
{
    protected readonly TManager _manager = manager;
    protected readonly ILogger _logger = logger;
    protected readonly IUserContext _user = user;
}

/// <summary>
/// RestApi base
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class RestControllerBase(Localizer localizer) : ControllerBase
{
    protected readonly Localizer _localizer = localizer;

    /// <summary>
    /// 自定义403
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public ObjectResult Forbid(string? value)
    {
        var res = CreateResult("Forbidden", value, 403);
        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseError", value));
        return new ObjectResult(res) { StatusCode = 403 };
    }

    /// <summary>
    /// 404返回格式处理
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    [NonAction]
    public NotFoundObjectResult NotFound(string? value)
    {
        var res = CreateResult("NotFound", value, 404);
        Activity? at = Activity.Current;
        return base.NotFound(res);
    }

    /// <summary>
    /// 409返回格式处理
    /// </summary>
    /// <returns></returns>
    [NonAction]
    public ConflictObjectResult Conflict(string? detail)
    {
        var res = CreateResult("Conflict", detail, 409);

        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseError", detail));
        return base.Conflict(res);
    }

    /// <summary>
    /// 500业务错误
    /// </summary>
    /// <param name="detail"></param>
    /// <param name="errorCode"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    [NonAction]
    public ObjectResult Problem(string? detail = null, int errorCode = 0, params object[] arguments)
    {
        var res = CreateResult("Problem", detail, errorCode, arguments);

        Activity? at = Activity.Current;
        _ = (at?.SetTag("responseError", detail));

        return new ObjectResult(res) { StatusCode = 500 };
    }

    /// <summary>
    /// 400返回格式处理
    /// </summary>
    /// <param name="error"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    [NonAction]
    public BadRequestObjectResult BadRequest(string? error, params object[] arguments)
    {
        var res = CreateResult(_localizer.Get(Localizer.BadRequest), error, 0, arguments);
        return base.BadRequest(res);
    }

    [NonAction]
    private ErrorResult CreateResult(
        string title,
        string? detail = null,
        int errorCode = 0,
        params object[] arguments
    )
    {
        var error = detail ?? string.Empty;

        if (detail != null)
        {
            error = _localizer.Get(detail, arguments) ?? detail;
        }
        else if (errorCode != 0)
        {
            error = _localizer.Get(errorCode.ToString()) ?? error;
        }

        return new ErrorResult(error, HttpContext.TraceIdentifier, title, errorCode);
    }
}
