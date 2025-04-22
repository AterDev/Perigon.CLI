using IdentityServer.Definition.Entity;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer.Definition.EntityFramework;

public class IdentityServerContext : DbContext
{
    public IdentityServerContext(DbContextOptions<IdentityServerContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; }

}
