namespace EntityFramework.AppDbContext;

/// <summary>
///  analysis-related readonly data access.
/// </summary>
/// <param name="options">The options to be used by the context. Must not be null.</param>
public class AnalysisDbContext(DbContextOptions<AnalysisDbContext> options)
    : ReadonlyDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
    }
}
