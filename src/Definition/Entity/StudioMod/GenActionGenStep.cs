using System.ComponentModel.DataAnnotations.Schema;

namespace Entity.StudioMod;
/// <summary>
/// 操作与步骤中间表
/// </summary>
public class GenActionGenStep
{
    public Guid GenActionsId { get; set; } = default!;
    [ForeignKey(nameof(GenActionsId))]
    public GenAction GenAction { get; set; } = null!;


    public Guid GenStepsId { get; set; } = default!;
    [ForeignKey(nameof(GenStepsId))]
    public GenStep GenStep { get; set; } = null!;
}
