namespace Ater.Common.Models;
/// <summary>
/// http在请求返回的错误格式
/// </summary>
public class HttpResponseError
{
    public string Title { get; set; } = string.Empty;
    public string? Detail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
