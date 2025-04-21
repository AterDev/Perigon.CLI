using Ater.Common.Utils;

using Entity.OrderMod;
using Entity.SystemMod;

namespace Entity.CustomerMod;
/// <summary>
/// 客户信息
/// </summary>
[Index(nameof(Name))]
[Index(nameof(CustomerType))]
[Index(nameof(IsFormal))]
[Index(nameof(Numbering), IsUnique = true)]
[Index(nameof(ContactInfo))]
[Index(nameof(FollowUpStatus))]
[Module(Modules.Customer)]
public class CustomerInfo : EntityBase
{
    #region 关联属性
    /// <summary>
    /// 关联账号
    /// </summary>
    public CustomerAccount? Account { get; set; }

    /// <summary>
    /// 添加人
    /// </summary>
    [ForeignKey(nameof(CreatedUserId))]
    public SystemUser CreatedUser { get; set; } = null!;
    public Guid CreatedUserId { get; set; }

    /// <summary>
    /// 管理人
    /// </summary>
    [ForeignKey(nameof(ManagerId))]
    public SystemUser Manager { get; set; } = null!;
    public Guid ManagerId { get; set; }

    #endregion

    #region 公司字段

    public CompanyType CompanyType { get; set; }

    /// <summary>
    /// 统一社会信用代码
    /// </summary>
    [MaxLength(60)]
    public string? UnifiedCode { get; set; }

    /// <summary>
    /// 法人
    /// </summary>
    [MaxLength(50)]
    public string? LegalPerson { get; set; }

    /// <summary>
    /// 发票类型
    /// </summary>
    public InvoiceType? InvoiceType { get; set; }

    /// <summary>
    /// 银行开户行
    /// </summary>
    [MaxLength(100)]
    public string? BankName { get; set; }

    /// <summary>
    /// 银行账户
    /// </summary>
    [MaxLength(50)]
    public string? BankAccount { get; set; }
    #endregion

    /// <summary>
    /// 姓名/真实姓名
    /// </summary>
    [MaxLength(40)]
    public required string Name { get; set; }

    /// <summary>
    /// 唯一编号
    /// </summary>
    [MaxLength(40)]
    public string Numbering { get; set; } = "S" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + HashCrypto.GetRnd(6);

    /// <summary>
    /// 真实姓名
    /// </summary>
    [MaxLength(40)]
    public string? RealName { get; set; }
    /// <summary>
    /// 生日
    /// </summary>
    public DateTimeOffset? BirthDay { get; set; }

    /// <summary>
    /// 年龄
    /// </summary>
    [NotMapped]
    public int Age
    {
        get
        {
            if (BirthDay.HasValue)
            {
                var today = DateTimeOffset.UtcNow;
                var age = today.Year - BirthDay.Value.Year;
                if (BirthDay.Value > today.AddYears(-age))
                {
                    age--;
                }
                return age;
            }
            return 0;
        }
    }

    public GenderType GenderType { get; set; } = GenderType.Male;

    /// <summary>
    /// 是否是正式客户
    /// </summary>
    public bool IsFormal { get; set; }

    /// <summary>
    /// 客户类型
    /// </summary>
    public CustomerType CustomerType { get; set; }

    /// <summary>
    /// 跟进状态
    /// </summary>
    public FollowUpStatus FollowUpStatus { get; set; } = FollowUpStatus.PendingTrial;

    /// <summary>
    /// 客户来源
    /// </summary>
    [MaxLength(60)]
    public string? Source { get; set; }

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
    /// 联系人
    /// </summary>
    [MaxLength(60)]
    public string? ContactName { get; set; }
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
    /// 关联的登记信息
    /// </summary>
    public CustomerRegister? CustomerRegister { get; set; }
    public Guid? CustomerRegisterId { get; set; }

    /// <summary>
    /// 标签
    /// </summary>
    public List<string>? Tags { get; set; } = [];
}

/// <summary>
/// 客户类型
/// </summary>
public enum CustomerType
{
    // TODO:定义类型 

    /// <summary>
    /// 个人
    /// </summary>
    [Description("个人")]
    Personal,

    /// <summary>
    /// 企业
    /// </summary>
    [Description("企业")]
    Company
}

/// <summary>
/// 客户跟进状态
/// </summary>
public enum FollowUpStatus
{
    /// <summary>
    /// 待试用
    /// </summary>
    [Description("待试用")]
    PendingTrial,

    /// <summary>
    /// 试用后第一次跟进
    /// </summary>
    [Description("试用后第一次跟进")]
    FirstFollowUpAfterTrial,

    /// <summary>
    /// 试用后第二次跟进
    /// </summary>
    [Description("试用后第二次跟进")]
    SecondFollowUpAfterTrial,

    /// <summary>
    /// 试用后第三次跟进
    /// </summary>
    [Description("试用后第三次跟进")]
    ThirdFollowUpAfterTrial,

    /// <summary>
    /// 高概率将合作
    /// </summary>
    [Description("高概率将合作")]
    HighProbabilityOfEnrollment,

    /// <summary>
    /// 有合作意愿但后期再考虑
    /// </summary>
    [Description("有合作意愿但后期再考虑")]
    InterestedButConsideringLater,

    /// <summary>
    /// 续费客户
    /// </summary>
    [Description("续费客户")]
    Renewal
}