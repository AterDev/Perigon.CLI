namespace Ater.AspNetCore.Models;

public class ErrorResult(string detail, string traceId, string title = "error", int status = 500)
{
    public string Title { get; set; } = title;
    public string? Detail { get; set; } = detail;
    public int Status { get; set; } = status;
    public string TraceId { get; set; } = traceId;
}
