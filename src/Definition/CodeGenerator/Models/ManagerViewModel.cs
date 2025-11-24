namespace CodeGenerator.Models;

public class ManagerViewModel
{
    public string? Namespace { get; set; }
    public string? EntityName { get; set; }
    public string? EntitySummary { get; set; }
    public string? DbContextName { get; set; }
    public string? ShareNamespace { get; set; }
    public string? AddMethod { get; set; }
    public string? FilterMethod { get; set; }
    public string? Comment { get; set; }

    /// <summary>
    /// 额外方法
    /// </summary>
    public string? AdditionMethods { get; set; }
}
