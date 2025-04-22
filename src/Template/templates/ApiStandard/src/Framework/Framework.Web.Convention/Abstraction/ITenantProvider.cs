namespace Framework.Web.Convention.Abstraction;
public interface ITenantProvider
{
    public Guid TenantId { get; set; }
}
