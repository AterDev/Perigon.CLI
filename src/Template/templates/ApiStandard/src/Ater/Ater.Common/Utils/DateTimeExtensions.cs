namespace Ater.Common.Utils;

/// <summary>
/// 日期时间扩展
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// dateOnly(local)转DateTimeOffset(utc)
    /// </summary>
    /// <param name="dateOnly"></param>
    /// <param name="zone"></param>
    /// <returns></returns>
    public static DateTimeOffset ToDateTimeOffset(this DateOnly dateOnly)
    {
        var localDateTime = dateOnly.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var utcDateTime = localDateTime.ToUniversalTime();
        return new DateTimeOffset(utcDateTime, TimeSpan.Zero);
    }

    public static DateTimeOffset ToDateTimeOffset(this DateTime dateTime, TimeZoneInfo? zone = null)
    {
        zone ??= TimeZoneInfo.Local;
        return new DateTimeOffset(dateTime, zone.GetUtcOffset(dateTime));
    }

    /// <summary>
    /// ToDateOnly
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static DateOnly ToDateOnly(this DateTimeOffset dateTimeOffset)
    {
        return DateOnly.FromDateTime(dateTimeOffset.DateTime);
    }

    /// <summary>
    /// 返回与现实对应的星期
    /// </summary>
    /// <param name="dateTimeOffset"></param>
    /// <returns></returns>
    public static int GetWeekDay(this DateTimeOffset dateTimeOffset)
    {
        DayOfWeek weekday = dateTimeOffset.DayOfWeek;
        return weekday switch
        {
            DayOfWeek.Sunday => 7,
            DayOfWeek.Monday => 1,
            DayOfWeek.Tuesday => 2,
            DayOfWeek.Wednesday => 3,
            DayOfWeek.Thursday => 4,
            DayOfWeek.Friday => 5,
            DayOfWeek.Saturday => 6,
            _ => throw new ArgumentOutOfRangeException(),
        };
    }
}
