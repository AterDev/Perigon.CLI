namespace SystemMod.Models;

public class UserInfoDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = default!;
    public string[] Roles { get; set; } = default!;
    public List<SystemMenu>? Menus { get; set; }
    public List<SystemPermissionGroup>? PermissionGroups { get; set; }
}
