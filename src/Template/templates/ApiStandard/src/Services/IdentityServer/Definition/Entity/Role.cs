using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Definition.Entity;

/// <summary>
/// 角色
/// </summary>
public class Role : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    public ICollection<AccountRole> AccountRoles { get; set; } = new List<AccountRole>();
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
