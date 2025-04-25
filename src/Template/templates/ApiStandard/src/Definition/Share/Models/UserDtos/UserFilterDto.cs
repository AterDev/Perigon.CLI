using Entity.UserMod;
using Framework.Common.Models;

namespace Share.Models.UserDtos;
/// <summary>
/// 用户账户查询筛选
/// </summary>
/// <inheritdoc cref="Entity.UserMod.User"/>
public class UserFilterDto : FilterBase
{
    /// <summary>
    /// 用户名
    /// </summary>
    [MaxLength(40)]
    public string? UserName { get; set; }
    /// <summary>
    /// 用户类型
    /// </summary>
    public UserType? UserType { get; set; }
    /// <summary>
    /// 邮箱
    /// </summary>
    [MaxLength(100)]
    public string? Email { get; set; }
    public bool? EmailConfirmed { get; set; }
    public string? PhoneNumber { get; set; }
    public bool? PhoneNumberConfirmed { get; set; }

}
