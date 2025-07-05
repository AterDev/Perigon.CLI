using System.ComponentModel.DataAnnotations;

namespace AterStudio.Components;

public class AddProjectDto
{
    [MaxLength(50)]
    public required string ProjectName { get; set; }

    [MinLength(3)]
    [MaxLength(200)]
    public required string ProjectDirectory { get; set; }
}