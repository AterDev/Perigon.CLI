using System.Text.Encodings.Web;
using System.Text.Unicode;
using System.Threading.RateLimiting;
using Ater.Common;
using Ater.Common.Converters;
using Ater.Common.Utils;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using ServiceDefaults.Middleware;

namespace ServiceDefaults;

public static class WebExtensions
{
    /// <summary>
    /// 注册和配置Web服务依赖
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddMiddlewareServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigureWebMiddleware(builder.Configuration);
        builder
            .Services.AddControllers()
            .ConfigureApiBehaviorOptions(o =>
            {
                o.InvalidModelStateResponseFactory = context =>
                {
                    return new CustomBadRequest(context, null);
                };
            })
            .AddJsonOptions(options =>
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
    public static IServiceCollection ConfigureWebMiddleware(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddJwtAuthentication(configuration);
        services.AddThirdAuthentication(configuration);

        services.AddAuthorize();
        services.AddCors();
        services.AddRateLimiter();

        services.AddOutputCache(options =>
        {
            options.AddPolicy("openapi", policy => policy.Expire(TimeSpan.FromMinutes(10)));
        });

        services.AddOpenApi(
            "v1",
            options =>
            {
                options.AddSchemaTransformer<OpenApiSchemaTransformer>();
                options.AddOperationTransformer<OpenApiOperationTransformer>();
            }
        );

        services.AddLocalizer();
        return services;
    }

    public static WebApplication UseMiddlewareServices(this WebApplication app)
    {
        // 异常统一处理
        app.UseExceptionHandler(ExceptionHandler.Handler());

        if (app.Environment.IsProduction())
        {
            app.UseCors(AppConst.Default);
            app.UseHsts();
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseCors(AppConst.Default);
            app.MapOpenApi().CacheOutput("openapi");
        }

        app.UseRateLimiter();
        app.UseStaticFiles();
        app.UseRequestLocalization();
        app.UseRouting();
        app.UseOutputCache();
        //app.UseMiddleware<JwtMiddleware>();
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
            options.AddPolicy(
                WebConst.Limited,
                context =>
                {
                    var remoteIpAddress = context.Connection.RemoteIpAddress;
                    if (!IPAddress.IsLoopback(remoteIpAddress!))
                    {
                        return RateLimitPartition.GetFixedWindowLimiter(
                            remoteIpAddress!.ToString(),
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 5,
                                Window = TimeSpan.FromSeconds(10),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = 3,
                            }
                        );
                    }
                    else
                    {
                        return RateLimitPartition.GetNoLimiter(remoteIpAddress!.ToString());
                    }
                }
            );

            // 全局限制 每10秒100次
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
            {
                IPAddress? remoteIpAddress = context.Connection.RemoteIpAddress;

                if (!IPAddress.IsLoopback(remoteIpAddress!))
                {
                    return RateLimitPartition.GetFixedWindowLimiter(
                        remoteIpAddress!,
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromSeconds(10),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 3,
                        }
                    );
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
            // 添加更多语言支持
            var supportedCultures = new[] { "zh-CN", "en-US" };
            options
                .SetDefaultCulture(supportedCultures[0])
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures);
            options.FallBackToParentCultures = true;
            options.FallBackToParentUICultures = true;
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        services.AddSingleton<Localizer>();
        return services;
    }

    /// <summary>
    /// 添加 jwt 验证
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services
            .AddAuthentication(options =>
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
                    ValidateIssuerSigningKey = true,
                };
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);
        return services;
    }

    /// <summary>
    /// 添加第三方认证（如微软）
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddThirdAuthentication(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection("Authentication");
        var msClientId = section.GetValue<string>("Microsoft:ClientId");
        var msClientSecret = section.GetValue<string>("Microsoft:ClientSecret");
        var msCallBackUrl = section.GetValue<string>("Microsoft:CallbackUrl");

        if (Utils.NoEmptyItem(msClientId, msClientSecret, msCallBackUrl))
        {
            services
                .AddAuthentication()
                .AddMicrosoftAccount(
                    MicrosoftAccountDefaults.AuthenticationScheme,
                    options =>
                    {
                        options.AuthorizationEndpoint =
                            "https://login.microsoftonline.com/consumers/oauth2/v2.0/authorize";
                        options.TokenEndpoint =
                            "https://login.microsoftonline.com/consumers/oauth2/v2.0/token";
                        options.ClientId = msClientId!;
                        options.ClientSecret = msClientSecret!;
                        options.CallbackPath = msCallBackUrl;
                        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    }
                );
        }

        var googleClientId = section.GetValue<string>("Google:ClientId");
        var googleClientSecret = section.GetValue<string>("Google:ClientSecret");
        var googleCallBackUrl = section.GetValue<string>("Google:CallbackUrl");
        if (Utils.NoEmptyItem(googleClientId, googleClientSecret, googleCallBackUrl))
        {
            services
                .AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = googleClientId!;
                    options.ClientSecret = googleClientSecret!;
                    options.CallbackPath = googleCallBackUrl;
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                });
        }

        return services;
    }

    public static IServiceCollection AddCors(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var section = configuration.GetSection("Cors");
        //get origins array
        var origins = section?.GetValue<string[]>("AllowedOrigins") ?? [];

        var allowedSubdomains = section?.GetValue<bool>("AllowedSubdomains") ?? false;

        services.AddCors(options =>
        {
            options.AddPolicy(
                AppConst.Default,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
            options.AddPolicy(
                WebConst.Limited,
                builder =>
                {
                    builder.WithOrigins(origins).AllowAnyMethod().AllowAnyHeader();
                    if (allowedSubdomains)
                    {
                        builder.SetIsOriginAllowedToAllowWildcardSubdomains();
                    }
                }
            );
        });
        return services;
    }

    public static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(WebConst.Default, policy => policy.RequireAuthenticatedUser())
            .AddPolicy(
                WebConst.User,
                policy => policy.RequireRole(WebConst.User, WebConst.AdminUser, WebConst.SuperAdmin)
            );

        return services;
    }
}
