
namespace Ater.AspNetCore.Abstraction;

public interface ITenantContext
{
    public Guid TenantId { get; set; }

    public string TenantType { get; set; }

    public string GetTenantName();
    public string GetDbConnectionString();
    public string GetAnalysisConnectionString();
}
