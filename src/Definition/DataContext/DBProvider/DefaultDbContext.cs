namespace DataContext.DBProvider;

public class DefaultDbContext() : MiniDbContext()
{
    public DbSet<Solution> Solutions { get; set; } = null!;
    public DbSet<GenAction> GenActions { get; set; } = null!;
    public DbSet<GenStep> GenSteps { get; set; } = null!;
    public DbSet<ConfigData> Configs { get; set; } = null!;
    public DbSet<GenActionGenStep> GenActionGenSteps { get; set; } = null!;
    public DbSet<McpTool> McpTools { get; set; } = null!;
    public DbSet<ApiDocInfo> ApiDocInfos { get; set; } = null!;
}
