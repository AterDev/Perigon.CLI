using Framework.Common.Models;

namespace CustomerMod.Models.CustomerInfoDtos;
/// <summary>
/// 客户信息列表元素
/// </summary>
/// <see cref="Entity.CustomerMod.CustomerInfo"/>
public class CustomerInfoItemDto
{
    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(40)]
    public string? RealName { get; set; }
    [MaxLength(40)]
    public string Name { get; set; } = default!;

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
    /// 客户类型
    /// </summary>
    public CustomerType? CustomerType { get; set; }

    /// <summary>
    /// 跟进状态
    /// </summary>
    public FollowUpStatus? FollowUpStatus { get; set; }
    /// <summary>
    /// 联系信息
    /// </summary>
    [MaxLength(100)]
    public string? ContactInfo { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; }
}
