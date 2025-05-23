using Ater.Common.Models;

namespace CustomerMod.Models.CustomerInfoDtos;
/// <summary>
/// 客户信息更新时请求结构
/// </summary>
/// <see cref="Entity.CustomerMod.CustomerInfo"/>
public class CustomerInfoUpdateDto
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(40)]
    public string? RealName { get; set; }
    /// <summary>
    /// 生日
    /// </summary>
    public DateTimeOffset? BirthDay { get; set; }
    public GenderType? GenderType { get; set; }
    /// <summary>
    /// 地址
    /// </summary>
    [MaxLength(300)]
    public string? Address { get; set; }
    /// <summary>
    /// 说明备注
    /// </summary>
    [MaxLength(2000)]
    public string? Remark { get; set; }
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
    public Guid? CustomerAccountId { get; set; }
    public List<Guid>? CustomerTagsIds { get; set; }

}
