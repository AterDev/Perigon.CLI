

namespace Ater.AspNetCore.Abstraction;

public interface ITenantContext
{
    public Guid TenantId { get; set; }

    public string TenantType { get; set; }

    Task<string?> GetDbConnectionStringAsync();
    Task<string?> GetAnalysisConnectionStringAsync();
    Task<string?> GetTenantNameAsync();
}
