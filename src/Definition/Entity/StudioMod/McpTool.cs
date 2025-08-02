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

    [MaxLength(300)]
    public required string PromptPath { get; set; }

    public string[] TemplatePaths { get; set; } = [];

    public Solution Project { get; set; } = default!;

    [ForeignKey(nameof(Project))]
    public Guid ProjectId { get; set; }
}
