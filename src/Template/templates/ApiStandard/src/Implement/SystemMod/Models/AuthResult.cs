using Entity.SystemMod;

namespace SystemMod.Models;

public class AuthResult
{
    public Guid Id { get; set; }
    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = default!;
    public string[] Roles { get; set; } = default!;

    public List<SystemMenu>? Menus { get; set; }
    /// <summary>
    /// token
    /// </summary>
    public string Token { get; set; } = default!;
    public List<SystemPermissionGroup>? PermissionGroups { get; set; }
}
