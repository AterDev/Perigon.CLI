using Ater.Common.Utils;
using Entity.SystemMod;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace MigrationService;

public class Worker(
    IServiceProvider serviceProvider,
    IHostApplicationLifetime hostApplicationLifetime,
    ILogger<Worker> logger
    ) : BackgroundService
{
    private readonly ILogger<Worker> _logger = logger;
    public const string ActivitySourceName = "Migrations";
    private static readonly ActivitySource _activitySource = new(ActivitySourceName);

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        using var activity = _activitySource.StartActivity("Migrating database", ActivityKind.Client);
        try
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CommandDbContext>();

            await RunMigrationAsync(dbContext, cancellationToken);
            await SeedDataAsync(dbContext, cancellationToken);
        }
        catch (Exception ex)
        {
            activity?.AddException(ex);
            throw;
        }
        hostApplicationLifetime.StopApplication();
    }
    private static async Task RunMigrationAsync(CommandDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await dbContext.Database.MigrateAsync(cancellationToken);
        });
    }

    private async Task SeedDataAsync(CommandDbContext dbContext, CancellationToken cancellationToken)
    {
        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // 初始化用户
            var user = await dbContext.SystemUsers.FirstOrDefaultAsync();
            if (user == null)
            {
                await InitUserAsync(dbContext);
            }
        });
    }

    /// <summary>
    /// 初始化角色
    /// </summary>
    public async Task InitUserAsync(CommandDbContext context)
    {
        string defaultPassword = "Hello.Net";
        var salt = HashCrypto.BuildSalt();

        var superAdmin = new SystemUser
        {
            UserName = "administrator",
            Email = "admin@dusi.dev",
            PasswordSalt = salt,
            PasswordHash = HashCrypto.GeneratePwd(defaultPassword, salt),
            SystemRoles = []
        };
        var role = new SystemRole
        {
            Name = "SuperAdmin",
            NameValue = "SuperAdmin",
        };
        superAdmin.SystemRoles.Add(role);
        try
        {
            context.SystemUsers.Add(superAdmin);
            await context.SaveChangesAsync();
            _logger.LogInformation("🎉 初始化管理员成功:{username}/{password}", superAdmin.UserName, defaultPassword);

        }
        catch (Exception ex)
        {
            _logger.LogError("初始化角色用户时出错,请确认您的数据库没有数据！{message}", ex.Message);
        }
    }
}