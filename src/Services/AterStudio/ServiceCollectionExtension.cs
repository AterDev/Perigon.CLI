using AterStudio.Components;
using AterStudio.Services;
using Microsoft.AspNetCore.Localization;

namespace AterStudio;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 注册和配置Web服务依赖
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddMiddlewareServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureWebMiddleware(builder.Configuration);
        return builder.Services;
    }

    public static WebApplication UseMiddlewareServices(this WebApplication app)
    {
        app.UseRequestLocalization();
        app.UseRouting();
        app.UseStaticFiles();
        app.UseAntiforgery();
        app.MapStaticAssets();



        app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

        return app;
    }

    /// <summary>
    /// 添加web服务组件，如身份认证/授权/swagger/cors
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureWebMiddleware(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddLocalizer();
        return services;
    }

    /// <summary>
    /// 添加本地化支持
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddLocalizer(this IServiceCollection services)
    {
        services.AddLocalization();
        services.AddRequestLocalization(options =>
        {
            var supportedCultures = new[] { "zh-CN", "en-US" };
            options
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;

            // 配置文化提供程序 - Cookie 优先
            options.RequestCultureProviders.Clear();
            options.RequestCultureProviders.Add(new CookieRequestCultureProvider());
            options.RequestCultureProviders.Add(new AcceptLanguageHeaderRequestCultureProvider());
        });

        services.AddSingleton<Localizer>();
        services.AddScoped<CultureService>();
        return services;
    }

    /// <summary>
    /// 添加blazor服务组件
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddBlazorServices(this WebApplicationBuilder builder)
    {
        builder
            .Services.AddRazorComponents(options =>
            {
                options.DetailedErrors = true;
            })
            .AddInteractiveServerComponents();
        builder.Services.AddFluentUIComponents();
        return builder.Services;
    }
}
