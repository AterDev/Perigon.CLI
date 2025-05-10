using Microsoft.Extensions.Hosting;

namespace StudioMod;

/// <summary>
/// 服务注册扩展
/// </summary>
public static partial class AppServiceCollectionExtensions
{
    /// <summary>
    /// 添加默认应用组件
    /// pgsql/redis/otlp
    /// </summary>
    /// <returns></returns>
    public static IHostApplicationBuilder AddDefaultComponents(this IHostApplicationBuilder builder)
    {
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
