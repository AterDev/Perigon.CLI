using System.ComponentModel.DataAnnotations;

namespace IdentityServer.Definition.Entity;

/// <summary>
/// 权限
/// </summary>
public class Permission : EntityBase
{
    [MaxLength(100)]
    public required string Name { get; set; }
    [MaxLength(200)]
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
