using CodeGenerator.Helper;
using Entity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Share;

/// <summary>
/// 服务注册扩展
/// </summary>
public static partial class FrameworkExtensions
{
    /// <summary>
    /// 添加默认应用组件
    /// pgsql/redis/otlp
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddFrameworkServices(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddScoped<UserContext>();

        builder.AddDbContext();
        builder.Services.AddMemoryCache();
        return builder;
    }

    /// <summary>
    /// 添加数据库上下文
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDbContext(this IHostApplicationBuilder builder)
    {
        builder.Services.AddScoped(typeof(DataAccessContext<>));
        builder.Services.AddScoped(typeof(DataAccessContext));

        var dir = AssemblyHelper.GetStudioPath();
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        var path = Path.Combine(dir, ConstVal.DbName);
        builder.Services.AddDbContext<CommandDbContext>(options =>
        {
            options.UseSqlite($"DataSource={path}", _ =>
            {
                _.MigrationsAssembly("AterStudio");
            });
        });
        builder.Services.AddDbContext<QueryDbContext>(options =>
        {
            options.UseSqlite($"DataSource={path}", _ =>
            {
                _.MigrationsAssembly("AterStudio");
            });
        });
        return builder;
    }
}
