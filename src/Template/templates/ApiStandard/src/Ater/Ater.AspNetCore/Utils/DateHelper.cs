namespace Ater.AspNetCore.Utils;

/// <summary>
/// 日期帮助类
/// </summary>
public class DateHelper
{
    /// <summary>
    /// 获取本周日期范围
    /// </summary>
    /// <returns></returns>
    public static (DateOnly startDate, DateOnly endDate) GetCurrentWeekDate()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var weekDay = now.GetWeekDay();
        var startDate = now.AddDays(-weekDay + 1).ToDateOnly();
        var endDate = now.AddDays(7 - weekDay).ToDateOnly();
        return (startDate, endDate);
    }

    /// <summary>
    /// 获取本周星期对应的日期
    /// </summary>
    /// <param name="weekday"></param>
    /// <returns></returns>
    public static DateOnly GetDateByWeekday(int weekday)
    {
        var now = DateTimeOffset.UtcNow;
        var today = now.GetWeekDay();
        var date = now.AddDays(weekday - today).ToDateOnly();
        return date;
    }

    /// <summary>
    /// 获取本月日期范围
    /// </summary>
    /// <returns></returns>
    public static (DateOnly startDate, DateOnly endDate) GetCurrentMonthDate()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var startDate = new DateOnly(now.Year, now.Month, 1);
        DateOnly endDate = startDate.AddMonths(1).AddDays(-1);
        return (startDate, endDate);
    }

    /// <summary>
    /// 获取本月最后一个 dayOfWeek 对应的日期
    /// </summary>
    /// <param name="dayOfWeek"></param>
    /// <returns></returns>
    public static DateOnly GetMonthLastWeekdayDate(DayOfWeek dayOfWeek)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        var startDate = new DateOnly(now.Year, now.Month, 1);
        DateOnly endDate = startDate.AddMonths(1).AddDays(-1);

        while (endDate.DayOfWeek != dayOfWeek)
        {
            endDate = endDate.AddDays(-1);
        }

        return endDate;
    }
}
