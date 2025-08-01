using System.Text.Encodings.Web;
using System.Text.Unicode;
using Definition.Share;
using Http.API;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace Http.API;

public static class ServiceCollectionExtension
{
    /// <summary>
    /// 注册和配置Web服务依赖
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IServiceCollection AddDefaultWebServices(this WebApplicationBuilder builder)
    {
        builder.Services.ConfigWebComponents(builder.Configuration);
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddManager();

        builder.Services.AddSingleton(typeof(CacheService));
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
            });
        return builder.Services;
    }

    public static WebApplication UseDefaultWebServices(this WebApplication app)
    {
        // 异常统一处理
        app.UseExceptionHandler(ExceptionHandler.Handler());
        if (app.Environment.IsProduction())
        {
            app.UseCors(WebConst.Default);
            //app.UseHsts();
            app.UseHttpsRedirection();
        }
        else
        {
            app.UseCors(WebConst.Default);
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/client/swagger.json", name: "client");
            });
        }

        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        return app;
    }

    /// <summary>
    /// 添加web服务组件，如身份认证/授权/swagger/cors
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigWebComponents(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddSwagger();
        services.AddJwtAuthentication(configuration);
        services.AddAuthorize();
        services.AddCors();
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
                var sign = configuration.GetSection("Authentication")["Jwt:Sign"];
                if (string.IsNullOrEmpty(sign))
                {
                    throw new Exception("未找到有效的Jwt配置");
                }
                cfg.TokenValidationParameters = new TokenValidationParameters()
                {
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(sign)),
                    ValidIssuer = configuration.GetSection("Authentication")["Jwt:ValidIssuer"],
                    ValidAudience = configuration.GetSection("Authentication")[
                        "Jwt:ValidAudiences"
                    ],
                    ValidateIssuer = true,
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ValidateIssuerSigningKey = true,
                };
            });
        return services;
    }

    /// <summary>
    /// 添加swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    In = ParameterLocation.Header,
                    Description = "Please enter a valid token",
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    BearerFormat = "JWT",
                    Scheme = "Bearer",
                }
            );
            c.AddSecurityRequirement(
                new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer",
                            },
                        },
                        Array.Empty<string>()
                    },
                }
            );
            c.SwaggerDoc(
                "client",
                new OpenApiInfo
                {
                    Title = "MyProjectName client",
                    Description =
                        "Client API 文档. 更新时间:" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"),
                    Version = "v1",
                }
            );
            var xmlFiles = Directory.GetFiles(
                AppContext.BaseDirectory,
                "*.xml",
                SearchOption.TopDirectoryOnly
            );
            foreach (var item in xmlFiles)
            {
                try
                {
                    c.IncludeXmlComments(item, includeControllerXmlComments: true);
                }
                catch (Exception) { }
            }
            c.SupportNonNullableReferenceTypes();
            c.DescribeAllParametersInCamelCase();
            c.CustomOperationIds(
                (z) =>
                {
                    var descriptor = (ControllerActionDescriptor)z.ActionDescriptor;
                    return $"{descriptor.ControllerName}_{descriptor.ActionName}";
                }
            );
            c.SchemaFilter<EnumSchemaFilter>();
            c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
        });
        return services;
    }

    public static IServiceCollection AddCors(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy(
                WebConst.Default,
                builder =>
                {
                    builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                }
            );
        });
        return services;
    }

    public static IServiceCollection AddAuthorize(this IServiceCollection services)
    {
        services
            .AddAuthorizationBuilder()
            .AddPolicy(WebConst.User, policy => policy.RequireRole(WebConst.User))
            .AddPolicy(
                WebConst.AdminUser,
                policy => policy.RequireRole(WebConst.SuperAdmin, WebConst.AdminUser)
            )
            .AddPolicy(WebConst.SuperAdmin, policy => policy.RequireRole(WebConst.SuperAdmin));
        return services;
    }
}
