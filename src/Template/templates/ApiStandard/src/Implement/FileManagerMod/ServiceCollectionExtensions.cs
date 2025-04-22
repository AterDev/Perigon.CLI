// 本文件由 ater.dry工具自动生成.
using FileManagerMod.Managers;

namespace FileManagerMod;
/// <summary>
/// 服务注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加FileManagerMod 注入服务
    /// </summary>
    /// <param name="services"></param>
    public static void AddFileManagerModManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(FileDataManager));
        services.AddScoped(typeof(FolderManager));

    }
}

