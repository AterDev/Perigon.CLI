using Ater.Common.Models;

namespace CustomerMod.Models.CustomerInfoDtos;

/// <summary>
/// 客户信息添加时请求结构
/// </summary>
/// <see cref="CustomerInfo"/>
public class CustomerInfoAddDto
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(40)]
    public string? RealName { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// 编号
    /// </summary>
    [MaxLength(40)]
    public string? Numbering { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public CustomerType CustomerType { get; set; }

    /// <summary>
    /// 客户来源
    /// </summary>
    [MaxLength(60)]
    public string? Source { get; set; }

    /// <summary>
    /// 性别
    /// </summary>
    public GenderType GenderType { get; set; } = GenderType.Male;

    /// <summary>
    /// 说明备注
    /// </summary>
    [MaxLength(2000)]
    public string? Remark { get; set; }

    /// <summary>
    /// 微信联系
    /// </summary>
    [MaxLength(100)]
    public required string ContactInfo { get; set; }

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

    /// <summary>
    /// 销售顾问id
    /// </summary>
    public Guid ConsultantId { get; set; }
}
