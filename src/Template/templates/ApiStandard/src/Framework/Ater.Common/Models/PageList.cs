namespace Ater.Common.Models;
public class PageList<T>
{
    public int Count { get; set; }
    public List<T> Data { get; set; } = [];
    public int PageIndex { get; set; } = 1;
}
