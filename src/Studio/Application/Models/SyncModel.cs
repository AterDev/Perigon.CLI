using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioMod.Models;
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
