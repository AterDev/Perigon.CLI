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
    public string TemplatePathsJsonString
    {
        get
        {
            if (_templatePaths != null)
            {
                return JsonSerializer.Serialize(_templatePaths);
            }
            return field;
        }
        set
        {
            field = value;
            _templatePaths = null;
        }
    } = string.Empty;

    private string[]? _templatePaths;

    /// <summary>
    /// template paths
    /// </summary>
    [NotMapped]
    public string[] TemplatePaths
    {
        get
        {
            if (_templatePaths == null)
            {
                if (string.IsNullOrEmpty(TemplatePathsJsonString))
                    _templatePaths = [];
                else
                    _templatePaths = JsonSerializer.Deserialize<string[]>(TemplatePathsJsonString) ?? [];
            }
            return _templatePaths;
        }
        set
        {
            _templatePaths = value;
        }
    }

    /// <summary>
    /// project id
    /// </summary>
    public int ProjectId { get; set; }
}
