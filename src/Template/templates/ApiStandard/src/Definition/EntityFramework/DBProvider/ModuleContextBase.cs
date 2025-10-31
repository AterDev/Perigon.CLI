using Entity.CMSMod;

namespace EntityFramework.DBProvider;

public partial class DefaultDbContext
{
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
}
