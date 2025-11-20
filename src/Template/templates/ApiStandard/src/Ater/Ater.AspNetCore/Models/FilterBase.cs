namespace Ater.AspNetCore.Models;

/// <summary>
/// 过滤
/// </summary>
public class FilterBase
{
    public int PageIndex
    {
        get;
        set
        {
            field = value;
            if (value < 1)
            {
                field = 1;
            }
        }
    }

    /// <summary>
    /// max 10000
    /// </summary>
    public int PageSize
    {
        get;
        set
        {
            field = value;
            if (value > 10000)
            {
                field = 10000;
            }
            if (value < 0)
            {
                field = 0;
            }
        }
    }

    /// <summary>
    /// 排序,field=>是否正序
    /// </summary>
    public Dictionary<string, bool>? OrderBy { get; set; }
}
