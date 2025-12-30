using System.Text.Json;

namespace Entity.StudioMod;

/// <summary>
/// mcp tool
/// </summary>
public class McpTool : EntityBase
{
    [Required]
    [MaxLength(40)]
    [RegularExpression("^[a-z0-9_-]+$")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(300)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(300)]
    public string PromptPath { get; set; } = string.Empty;

    /// <summary>
    /// TemplatePaths stored as JSON string
    /// </summary>
    [MaxLength(1000)]
    public string TemplatePathsJsonString { get; set; } = string.Empty;

    /// <summary>
    /// template paths
    /// </summary>
    [NotMapped]
    public string[] TemplatePaths
    {
        get
        {
            if (string.IsNullOrEmpty(TemplatePathsJsonString))
                return [];

            return JsonSerializer.Deserialize<string[]>(TemplatePathsJsonString) ?? [];
        }
        set
        {
            if (value == null || value.Length == 0)
                TemplatePathsJsonString = string.Empty;
            else
                TemplatePathsJsonString = JsonSerializer.Serialize(value);
        }
    }

    /// <summary>
    /// project id
    /// </summary>
    public int ProjectId { get; set; }
}
