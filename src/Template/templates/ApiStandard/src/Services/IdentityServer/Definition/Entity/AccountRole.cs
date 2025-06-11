namespace IdentityServer.Definition.Entity;

/// <summary>
/// 用户-角色多对多关系
/// </summary>
public class AccountRole
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
}
