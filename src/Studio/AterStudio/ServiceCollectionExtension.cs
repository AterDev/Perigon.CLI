using System.Text.Encodings.Web;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using CodeGenerator.Models;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi.Models;
using Share.Services;
using StudioMod.Services;

namespace AterStudio;

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

        builder.Services.AddScoped<IProjectContext, ProjectContext>();

        builder.Services.AddScoped<CodeAnalysisService>();
        builder.Services.AddScoped<CodeGenService>();
        builder.Services.AddScoped<SolutionService>();

        builder.Services.AddManagers();

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
                options.JsonSerializerOptions.ReadCommentHandling = JsonCommentHandling.Skip;
            });
        return builder.Services;
    }

    public static WebApplication UseDefaultWebServices(this WebApplication app)
    {
        // 异常统一处理
        app.UseExceptionHandler(ExceptionHandler.Handler());
        app.UseCors(WebConst.Default);

        app.UseSwagger();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();
        app.MapFallbackToFile("index.html");

        return app;
    }

    /// <summary>
    /// 添加web服务组件，如身份认证/授权/swagger/cors
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection ConfigWebComponents(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
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
    /// 添加swagger服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddOpenApi(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("admin", new OpenApiInfo
            {
                Title = "AterStudio API doc",
                Description = "Studio API 文档. 更新时间:" + DateTime.Now.ToString("yyyy-MM-dd H:mm:ss"),
                Version = "v1"
            });
            var xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly);
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
            c.CustomOperationIds((z) =>
            {
                var descriptor = (ControllerActionDescriptor)z.ActionDescriptor;
                return $"{descriptor.ControllerName}_{descriptor.ActionName}";
            });
            c.SchemaFilter<EnumSchemaFilter>();
            c.MapType<DateOnly>(() => new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "date"
            });
        });
        return services;
    }
}
