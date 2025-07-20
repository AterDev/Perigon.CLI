namespace Entity.StudioMod;

/// <summary>
/// mcp tool
/// </summary>
public class McpTool : EntityBase
{
    [Required]
    [MaxLength(40)]
    public required string Name { get; set; }

    [Required]
    [MaxLength(300)]
    public required string Description { get; set; }

    [Required]
    [MaxLength(300)]
    public required string PromptPath { get; set; }

    public string[] TemplatePaths { get; set; } = [];
}
