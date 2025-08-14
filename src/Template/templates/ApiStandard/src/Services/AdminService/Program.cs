WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// 共享基础服务:health check, service discovery, opentelemetry, http retry etc.
builder.AddServiceDefaults();

// 框架依赖服务:options, cache, dbContext
builder.AddFrameworkServices();

// Web中间件服务:route, openapi, jwt, cors, auth, rateLimiter etc.
builder.AddMiddlewareServices();

builder
    .Services.AddAuthorizationBuilder()
    .AddPolicy(
        WebConst.AdminUser,
        policy =>
        {
            policy.RequireRole(WebConst.AdminUser, WebConst.SuperAdmin);
        }
    )
    .AddPolicy(
        WebConst.SuperAdmin,
        policy =>
        {
            policy.RequireRole(WebConst.SuperAdmin);
        }
    );

// 业务Managers
builder.Services.AddManagers();

// 模块服务
builder.AddModules();

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
        await SystemMod.InitModule.InitializeAsync(provider);
    }
    app.Run();
}
