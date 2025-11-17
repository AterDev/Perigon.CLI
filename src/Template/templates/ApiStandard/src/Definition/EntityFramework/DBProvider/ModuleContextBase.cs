using Entity.CMSMod;

namespace EntityFramework.DBProvider;

public partial class DefaultDbContext
{
    public DbSet<Article> Blogs { get; set; }
    public DbSet<Catalog> Catalogs { get; set; }
}
