namespace CodeGenerator.Models;

public class ManagerViewModel
{
    public string? Namespace { get; set; }
    public string? EntityName { get; set; }
    public string? DbContextName { get; set; }
    public string? ShareNamespace { get; set; }
    public string? FilterCode { get; set; }
    public string? Comment { get; set; }
}
