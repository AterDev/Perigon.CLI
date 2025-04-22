using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Framework.Web.Convention;

public static class ExceptionHandler
{
    public static Action<IApplicationBuilder> Handler()
    {
        return builder =>
        {
            builder.Run(async context =>
            {
                context.Response.StatusCode = 500;
                Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var result = new
                {
                    Title = "异常错误",
                    exception?.Source,
                    Detail = exception?.Message + exception?.InnerException?.Message,
                    exception?.StackTrace,
                    Status = 500,
                    TraceId = context.TraceIdentifier
                };
                await context.Response.WriteAsJsonAsync(result);
            });
        };
    }
}
