using System.Diagnostics;
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
                    Title = "Exception",
                    Detail = exception?.Message + exception?.InnerException?.Message,
                    Status = 500,
                    TraceId = context.TraceIdentifier
                };

                Activity? at = Activity.Current;

                _ = (at?.SetTag("responseBody", exception));

                await context.Response.WriteAsJsonAsync(result);
            });
        };
    }
}
