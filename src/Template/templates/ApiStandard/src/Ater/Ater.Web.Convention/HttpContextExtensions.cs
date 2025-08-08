using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Ater.Web.Convention;

/// <summary>
/// http context extensions
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// get ip address from request
    /// </summary>
    /// <param name="httpContext"></param>
    /// <returns></returns>
    public static string? GetClientIp(this HttpContext httpContext)
    {
        HttpRequest? request = httpContext.Request;
        return request == null ? string.Empty
            : request.Headers.TryGetValue("X-Forwarded-For", out StringValues value)
                ? value.Where(x => x != null).FirstOrDefault()
            : httpContext.Connection.RemoteIpAddress?.ToString();
    }
}
