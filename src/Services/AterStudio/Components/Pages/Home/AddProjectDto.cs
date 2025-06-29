using Microsoft.Build.Framework;

namespace AterStudio.Components;

public class AddProjectDto
{
    [Required] public string? ProjectName { get; set; }
    [Required] public string? ProjectDirectory { get; set; }
}