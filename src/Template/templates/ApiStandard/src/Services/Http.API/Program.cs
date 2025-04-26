using Http.API.Extensions;
using Http.API.Worker;
using ServiceDefaults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();
// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();
// 业务Managers
builder.Services.AddManagers();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();

// 自定义选项及服务
builder.Services.AddSingleton<IEmailService, EmailService>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// 使用中间件
app.UseDefaultWebServices();

using (app)
{
    // 初始化工作
    await using (AsyncServiceScope scope = app.Services.CreateAsyncScope())
    {
        IServiceProvider provider = scope.ServiceProvider;
        await InitDataTask.InitDataAsync(provider);
    }
    app.Run();
}

public partial class Program { }