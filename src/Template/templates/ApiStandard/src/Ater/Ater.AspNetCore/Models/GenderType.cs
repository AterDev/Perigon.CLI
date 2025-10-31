namespace Ater.AspNetCore.Models;

/// <summary>
/// Gender
/// </summary>
public enum GenderType
{
    /// <summary>
    /// 男性
    /// </summary>
    [Description("男性")]
    Male,

    /// <summary>
    /// 女性
    /// </summary>
    [Description("女性")]
    Female,

    /// <summary>
    /// 其他
    /// </summary>
    [Description("其他")]
    Else,
}
