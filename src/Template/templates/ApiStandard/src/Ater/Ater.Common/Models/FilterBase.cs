namespace Ater.Common.Models;
/// <summary>
/// 过滤
/// </summary>
public class FilterBase
{
    private int _pageSize;
    private int _pageIndex;

    public int PageIndex
    {
        get => _pageIndex;
        set
        {
            _pageIndex = value;
            if (value < 1)
            {
                _pageIndex = 1;
            }
        }
    }

    /// <summary>
    /// 默认最大1000
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set
        {
            _pageSize = value;
            if (value > 1000)
            {
                _pageSize = 1000;
            }
            if (value < 0)
            {
                _pageSize = 0;
            }
        }
    }

    /// <summary>
    /// 排序,field=>是否正序
    /// </summary>
    public Dictionary<string, bool>? OrderBy { get; set; }

}
