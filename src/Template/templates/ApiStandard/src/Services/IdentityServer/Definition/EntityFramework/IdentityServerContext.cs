using IdentityServer.Definition.Entity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Definition.EntityFramework;

public class IdentityServerContext : DbContext
{
    public IdentityServerContext(DbContextOptions<IdentityServerContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<AccountRole> AccountRoles { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // 配置多对多关系
        modelBuilder.Entity<AccountRole>().HasKey(ar => new { ar.AccountId, ar.RoleId });
        modelBuilder.Entity<RolePermission>().HasKey(rp => new { rp.RoleId, rp.PermissionId });
    }
}
