namespace Share.Models;
/// <summary>
/// 同步模型
/// </summary>
public class SyncModel
{
    public TemplateSync? TemplateSync { get; set; }

}

public class TemplateSync
{
    public List<GenAction> GenActions { get; set; } = [];
    public List<GenStep> GenSteps { get; set; } = [];
    public List<GenActionGenStep> GenActionGenSteps { get; set; } = [];
}
