namespace Entity.StudioMod;

/// <summary>
/// mcp tool
/// </summary>
public class McpTool : EntityBase
{
    [MaxLength(40)]
    public required string Name { get; set; }

    [MaxLength(300)]
    public required string Description { get; set; }
}
