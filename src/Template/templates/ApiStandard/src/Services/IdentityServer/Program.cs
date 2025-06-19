using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Components;
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

builder.Services.AddLocalization();
builder.Services.AddRequestLocalization(options =>
{
    // 添加更多语言支持
    var supportedCultures = new[] { "zh-CN", "en-US" };
    options.SetDefaultCulture(supportedCultures[0])
        .AddSupportedCultures(supportedCultures)
        .AddSupportedUICultures(supportedCultures);
    options.FallBackToParentCultures = true;
    options.FallBackToParentUICultures = true;
    options.ApplyCurrentCultureToResponseHeaders = true;
});

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddScoped<HttpClient>(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    return new HttpClient { BaseAddress = new Uri(nav.BaseUri) };
});

builder.Services.AddFluentUIComponents()
    .AddDataGridEntityFrameworkAdapter();

builder.Services.AddScoped<ApplicationManager>();
builder.Services.AddScoped<LoginManager>();
builder.Services.AddScoped<Localizer>();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
    {
        options.Cookie.Name = ".AspNetCore.IdentityServerCookie";
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";
    });

builder.Services.AddCascadingAuthenticationState();

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();


app.UseRouting();
app.UseRequestLocalization();

app.UseAntiforgery();
app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapDefaultEndpoints();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// initialize user & SuperAdmin role
await IdentityServer.Init.EnsureAdminAndSuperAdminAsync(app.Services);

app.Run();
