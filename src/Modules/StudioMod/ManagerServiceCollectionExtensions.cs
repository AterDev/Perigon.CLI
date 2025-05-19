using StudioMod.Managers;

namespace StudioMod;
public static partial class ManagerServiceCollectionExtensions
{
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        services.AddScoped(typeof(DataAccessContext<>));
        services.AddScoped<AdvanceManager>();
        services.AddScoped<ApiDocInfoManager>();
        services.AddScoped<EntityInfoManager>();
        services.AddScoped<GenActionManager>();
        services.AddScoped<GenStepManager>();
        services.AddScoped<ProjectManager>();
        services.AddScoped<SolutionManager>();
        services.AddScoped<ToolsManager>();

        return services;
    }
}