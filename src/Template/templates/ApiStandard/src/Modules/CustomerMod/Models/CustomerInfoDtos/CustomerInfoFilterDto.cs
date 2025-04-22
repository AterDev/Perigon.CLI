using Framework.Common.Models;

namespace CustomerMod.Models.CustomerInfoDtos;
/// <summary>
/// 客户信息查询筛选
/// </summary>
/// <see cref="Entity.CustomerMod.CustomerInfo"/>
public class CustomerInfoFilterDto : FilterBase
{
    /// <summary>
    /// 姓名
    /// </summary>
    [MaxLength(40)]
    public string? SearchKey { get; set; }
    public GenderType? GenderType { get; set; }

    /// <summary>
    /// 联系信息
    /// </summary>
    [MaxLength(100)]
    public string? ContactInfo { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public CustomerType? CustomerType { get; set; }

    /// <summary>
    /// 跟进状态
    /// </summary>
    public FollowUpStatus? FollowUpStatus { get; set; }

}
