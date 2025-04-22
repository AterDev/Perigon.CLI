namespace SystemMod.Models.SystemMenuDtos;
/// <summary>
/// <inheritdoc cref="SystemMenu"/>
/// </summary>
public class SystemMenuSyncDto
{
    public required string Name { get; set; }
    public required string AccessCode { get; set; }
    public int MenuType { get; set; }
    public int? Sort { get; set; }
    public string? Icon { get; set; }
    public List<SystemMenuSyncDto> Children { get; set; } = [];
}
