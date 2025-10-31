namespace Ater.AspNetCore.Abstraction;

public interface ITenantProvider
{
    public Guid TenantId { get; set; }
}
