// 本文件由 ater.dry工具自动生成.
namespace Application;
public static partial class AppManagerServiceExtensions
{
    public static void AddManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(UserManager));
    }
}
