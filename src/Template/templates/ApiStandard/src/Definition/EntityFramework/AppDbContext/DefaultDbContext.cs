using Entity.CMSMod;
using Entity.SystemMod;

namespace EntityFramework.AppDbContext;

/// <summary>
/// default data access for main business
/// </summary>
/// <param name="options"></param>
public partial class DefaultDbContext(DbContextOptions<DefaultDbContext> options)
    : ContextBase(options)
{
    public DbSet<Article> Articles { get; set; }
    public DbSet<ArticleCategory> ArticleCategories { get; set; }

    public DbSet<SystemUser> SystemUsers { get; set; }
    public DbSet<SystemRole> SystemRoles { get; set; }
    public DbSet<SystemUserRole> SystemUserRoles { get; set; }
    public DbSet<SystemConfig> SystemConfigs { get; set; }

    /// <summary>
    /// 菜单
    /// </summary>
    public DbSet<SystemMenu> SystemMenus { get; set; }
    public DbSet<SystemPermission> SystemPermissions { get; set; }

    /// <summary>
    /// 权限组
    /// </summary>
    public DbSet<SystemPermissionGroup> SystemPermissionGroups { get; set; }
    public DbSet<SystemLogs> SystemLogs { get; set; }
    public DbSet<SystemOrganization> SystemOrganizations { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SystemLogs>().Ignore(e => e.IsDeleted);

        // 配置SystemUser和SystemRole的多对多关系
        builder
            .Entity<SystemUser>()
            .HasMany(u => u.SystemRoles)
            .WithMany(r => r.Users)
            .UsingEntity<SystemUserRole>(
                j => j.HasOne(ur => ur.Role).WithMany().HasForeignKey(ur => ur.RoleId),
                j => j.HasOne(ur => ur.User).WithMany().HasForeignKey(ur => ur.UserId),
                j =>
                {
                    j.HasKey(ur => ur.Id);
                    j.ToTable("SystemUserRoles");
                }
            );
    }
}
