namespace IdentityServer.Definition.Entity;

/// <summary>
/// 角色-权限多对多关系
/// </summary>
public class RolePermission
{
    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;
}
