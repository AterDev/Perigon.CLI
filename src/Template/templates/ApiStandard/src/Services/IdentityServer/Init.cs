using Ater.Common.Utils;
using IdentityServer.Definition.Entity;

namespace IdentityServer;

public static class Init
{
    public static async Task EnsureAdminAndSuperAdminAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityServerContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Init");

        if (!await db.Database.CanConnectAsync())
        {
            logger.LogWarning("Database connection failed, skipping admin initialization.");
            return;
        }

        var adminUser = await db.Accounts.FirstOrDefaultAsync(a => a.UserName == ConstVal.DefaultAdminUserName);
        var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == ConstVal.SuperAdmin);

        if (adminUser == null || superAdminRole == null)
        {
            if (superAdminRole == null)
            {
                superAdminRole = new Role
                {
                    Name = ConstVal.SuperAdmin,
                    Description = ConstVal.SuperAdmin
                };
                db.Roles.Add(superAdminRole);
                await db.SaveChangesAsync();
            }
            if (adminUser == null)
            {
                var salt = HashCrypto.BuildSalt();
                var hash = HashCrypto.GeneratePwd(ConstVal.DefaultAdminUserName, salt);
                adminUser = new Account
                {
                    UserName = ConstVal.DefaultAdminUserName,
                    HashPassword = hash,
                    HashSalt = salt,
                    Email = "ater@dusi.dev",
                    AccountRoles = new List<AccountRole>()
                };
                db.Accounts.Add(adminUser);
                await db.SaveChangesAsync();
            }

            if (!db.AccountRoles.Any(ar => ar.AccountId == adminUser.Id && ar.RoleId == superAdminRole.Id))
            {
                db.AccountRoles.Add(new AccountRole
                {
                    AccountId = adminUser.Id,
                    RoleId = superAdminRole.Id
                });
                await db.SaveChangesAsync();
            }
            logger.LogInformation("Initialized user: admin, default password: admin");
        }
    }
}
