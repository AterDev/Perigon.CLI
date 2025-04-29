using Http.API.Extensions;
using Http.API.Worker;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();
// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// 业务Managers
builder.Services.AddManagers();
// 模块服务
builder.AddModules();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();

// 自定义选项及服务
builder.Services.AddSingleton<IEmailService, EmailService>();

WebApplication app = builder.Build();

app.MapDefaultEndpoints();

// 使用中间件
app.UseMiddlewareServices();

using (app)
{
    // 在启动前执行初始化操作
    await using (var scope = app.Services.CreateAsyncScope())
    {
        IServiceProvider provider = scope.ServiceProvider;
        await Initialize.InitAsync(provider);
    }
    app.Run();
}