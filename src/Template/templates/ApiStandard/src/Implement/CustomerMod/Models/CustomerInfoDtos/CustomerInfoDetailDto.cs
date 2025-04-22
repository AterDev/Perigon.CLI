using Framework.Common.Models;

namespace CustomerMod.Models.CustomerInfoDtos;
/// <summary>
/// 客户信息概要
/// </summary>
/// <see cref="Entity.CustomerMod.CustomerInfo"/>
public class CustomerInfoDetailDto
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(40)]
    public string RealName { get; set; } = default!;
    /// <summary>
    /// 生日
    /// </summary>
    public DateTimeOffset? BirthDay { get; set; }
    /// <summary>
    /// 年龄
    /// </summary>
    public int Age { get; set; }
    public GenderType GenderType { get; set; } = GenderType.Male;
    /// <summary>
    /// 地址
    /// </summary>
    [MaxLength(300)]
    public string? Address { get; set; }
    /// <summary>
    /// 联系信息
    /// </summary>
    [MaxLength(100)]
    public string? ContactInfo { get; set; }
    /// <summary>
    /// 联系电话
    /// </summary>
    [MaxLength(20)]
    public string? ContactPhone { get; set; }
    /// <summary>
    /// 联系邮箱
    /// </summary>
    [MaxLength(100)]
    public string? ContactEmail { get; set; }

}
