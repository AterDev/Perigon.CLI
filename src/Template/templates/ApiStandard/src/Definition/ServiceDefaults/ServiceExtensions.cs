using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using Framework.Common.Converters;
using Framework.Common.Options;
using Framework.Web.Convention;
using Framework.Web.Convention.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace ServiceDefaults;
public static class ServiceExtensions
{

    /// <summary>
    /// 注册和配置Web服务依赖
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureWebMiddleware(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(o =>
            {
                o.InvalidModelStateResponseFactory = context =>
                {
                    return new CustomBadRequest(context, null);
                };
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All);
                options.JsonSerializerOptions.Converters.Add(new DateOnlyJsonConverter());
            });
        return builder.Services;
    }

    /// <summary>
    /// 添加web服务组件，如身份认证/授权/swagger/cors
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigureWebMiddleware(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenAPI();
        services.AddJwtAuthentication(configuration);
        services.AddAuthorize();
        services.AddCORS();
        services.AddRateLimiter();
        return services;
    }


    public static WebApplication UseDefaultWebServices(this WebApplication app)
    {
        app.UseWebAppContext();
        // 异常统一处理
        app.UseExceptionHandler(ExceptionHandler.Handler());
        if (app.Environment.IsProduction())
        {
            app.UseCors(WebConst.Default);
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseCors(WebConst.Default);
        }

        app.UseRateLimiter();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseMiddleware<JwtMiddleware>();
        app.UseRequestLocalization();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }



    /// <summary>
    /// 添加速率限制
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            // 验证码  每10秒5次
            options.AddPolicy("captcha", context =>
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                if (!IPAddress.IsLoopback(remoteIpAddress!))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(remoteIpAddress!.ToString(), _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromSeconds(10),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 3
                    });
                }
                else
                {
                    return RateLimitPartition.GetNoLimiter(remoteIpAddress!.ToString());
                }
            });

            // 全局限制 每10秒100次
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

                if (!IPAddress.IsLoopback(remoteIpAddress!))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(remoteIpAddress!, _ =>
                    new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromSeconds(10),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 3
                    });
                }

                return RateLimitPartition.GetNoLimiter(IPAddress.Loopback);
            });
        });
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
            // TODO:添加更多语言支持
            var supportedCultures = new[] { "zh-CN", "en-US" };
            options.SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });
        return services;
    }

    /// <summary>
    /// 添加 jwt 验证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(cfg =>
        {
            cfg.SaveToken = true;
            var jwtOption = configuration.GetSection(JwtOption.ConfigPath).Get<JwtOption>();
            var sign = jwtOption?.Sign;
            if (string.IsNullOrEmpty(sign))
            {
                throw new Exception("未找到有效的Jwt配置");
            }
            cfg.TokenValidationParameters = new TokenValidationParameters()
            {
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sign)),
                ValidIssuer = jwtOption?.ValidIssuer,
                ValidAudience = jwtOption?.ValidAudiences,
                ValidateIssuer = true,
                ValidateLifetime = true,
                RequireExpirationTime = true,
                ValidateIssuerSigningKey = true
            };
        });
        return services;
    }

    /// <summary>
    /// 添加swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenAPI(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();

        return services;
    }

    public static IServiceCollection AddCORS(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(WebConst.Default, builder =>
            {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
        return services;
    }

    public static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(WebConst.User, policy => policy.RequireRole(WebConst.User))
            .AddPolicy(WebConst.AdminUser, policy => policy.RequireRole(WebConst.SuperAdmin, WebConst.AdminUser))
            .AddPolicy(WebConst.SuperAdmin, policy => policy.RequireRole(WebConst.SuperAdmin));
        return services;
    }
}
