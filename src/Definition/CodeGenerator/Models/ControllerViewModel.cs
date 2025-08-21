namespace CodeGenerator.Models;

/// <summary>
/// 控制器模板模型
/// </summary>
public class ControllerViewModel
{
    public string? Namespace { get; set; }
    public string? EntityName { get; set; }
    public string? ShareNamespace { get; set; }
    public string? Comment { get; set; }
    public string? FilterCodes { get; set; }
    public string? AddCodes { get; set; }
    public string? UpdateCodes { get; set; }
    public string? DetailCodes { get; set; }
    public string? DeleteCodes { get; set; }
}
