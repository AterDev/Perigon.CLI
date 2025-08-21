using Entity.CMSMod;
using Entity.CustomerMod;
using Entity.FileManagerMod;
using Entity.OrderMod;
using Entity.UserMod;

namespace EntityFramework.DBProvider;

public partial class DefaultDbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserConfig> UserConfigs { get; set; }
    public DbSet<UserLog> UserLogs { get; set; }

    public DbSet<FileData> FileDatas { get; set; }
    public DbSet<Folder> Folders { get; set; }

    public DbSet<Order> Orders { get; set; }
    public DbSet<Product> Products { get; set; }

    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }

    public DbSet<CustomerInfo> CustomerInfos { get; set; }
    public DbSet<CustomerRegister> CustomerRegisters { get; set; }
}
