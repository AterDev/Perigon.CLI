using System.ComponentModel.DataAnnotations;

namespace AterStudio.Components.Shared.Models;

public class AddProjectDto
{
    [MaxLength(50)]
    public required string ProjectName { get; set; }

    [MinLength(3)]
    [MaxLength(200)]
    public required string ProjectDirectory { get; set; }
}