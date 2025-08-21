using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Ater.Web.Convention;

public static class ExceptionHandler
{
    public static Action<IApplicationBuilder> Handler()
    {
        return builder =>
        {

            Activity? activity = Activity.Current;
            builder.Run(async context =>
            {
                context.Response.StatusCode = 500;
                Exception? exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
                var result = new
                {
                    Title = "Exception",
                    Detail = exception?.Message + exception?.InnerException?.Message,
                    Status = 500,
                    TraceId = activity?.TraceId.ToString() ?? context.TraceIdentifier
                };

                _ = (activity?.SetTag("responseBody", exception));
                await context.Response.WriteAsJsonAsync(result);
            });
        };
    }
}
