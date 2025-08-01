namespace Ater.Common.Models;
public class ErrorResult
{
    public required string Title { get; set; } = "error";
    public string? Detail { get; set; }
    public int Status { get; set; } = 500;
    public required string TraceId { get; set; }

}
