using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.StudioMod;
/// <summary>
/// generate log
/// </summary>
public class GenLog : EntityBase
{

    /// <summary>
    /// source path:local file path or url
    /// </summary>
    [MaxLength(300)]
    public string? SourcePath { get; set; }


    /// <summary>
    /// log content
    /// </summary>
    [MaxLength(2000)]
    public string? LogContent { get; set; }

    [ForeignKey(nameof(GenActionId))]
    public GenAction GenAction { get; set; } = null!;
    public Guid GenActionId { get; set; } = default!;

}
