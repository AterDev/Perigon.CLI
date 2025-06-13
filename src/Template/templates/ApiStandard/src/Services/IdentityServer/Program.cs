using IdentityServer.Components;
using IdentityServer.Definition.EntityFramework;
using ServiceDefaults;
using Microsoft.FluentUI.AspNetCore.Components; // 添加 FluentUI using

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddDbContext<IdentityServerContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("IdentityServerContext"));
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

builder.Services.AddControllers();

var app = builder.Build();

app.MapDefaultEndpoints();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();
app.MapStaticAssets();
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
