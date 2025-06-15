using IdentityServer.Components;
using IdentityServer.Definition.EntityFramework;
using IdentityServer.Managers; // 添加 FluentUI using
using Microsoft.FluentUI.AspNetCore.Components;
using ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddDbContext<IdentityServerContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityServer"));
    options.UseOpenIddict();
});

builder.Services.AddOpenIddict()
    .AddCore(options =>
    {
        options.UseEntityFrameworkCore()
            .UseDbContext<IdentityServerContext>();
    })
    .AddServer(options =>
    {
        options.SetAuthorizationEndpointUris("/connect/authorize")
            .SetTokenEndpointUris("/connect/token")
            .SetUserInfoEndpointUris("/connect/userinfo")
            .SetDeviceAuthorizationEndpointUris("/connect/device")
            .SetEndUserVerificationEndpointUris("/connect/verify")
            .AllowAuthorizationCodeFlow()
            .AllowClientCredentialsFlow()
            .AllowDeviceAuthorizationFlow()
            .AllowPasswordFlow()
            .AllowRefreshTokenFlow();

        options.RegisterScopes("openid", "profile", "email", "phone", "roles");

        options.AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();

        options.UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough()
            .EnableUserInfoEndpointPassthrough();
    })
    .AddValidation(options =>
    {
        options.UseLocalServer();
        options.UseAspNetCore();

    });

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddFluentUIComponents(); // 注册 FluentUI 组件服务

// 注册 ApplicationManager
builder.Services.AddScoped<ApplicationManager>();


// builder.Services.AddControllers(); // 移除 API 控制器注册

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
