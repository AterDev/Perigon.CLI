namespace Entity.StudioMod;

public class McpTool
{
    [MaxLength(40)]
    public required string Name { get; set; }

    [MaxLength(300)]
    public required string Description { get; set; }
}
