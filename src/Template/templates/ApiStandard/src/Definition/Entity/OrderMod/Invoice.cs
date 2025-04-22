using Framework.Common.Utils;

namespace Entity.OrderMod;
/// <summary>
/// 对账单
/// </summary>
[Index(nameof(InvoiceNumber), IsUnique = true)]
[Index(nameof(InvoiceDate))]
[Index(nameof(CustomerName))]
[Module(Modules.Order)]
public class Invoice : EntityBase
{
    /// <summary>
    /// 发票号码
    /// </summary>
    [MaxLength(30)]
    public string InvoiceNumber { get; set; } = "RC" + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + HashCrypto.GetRnd(4);

    /// <summary>
    /// 发票日期
    /// </summary>
    public DateOnly InvoiceDate { get; set; }

    /// <summary>
    /// 客户姓名
    /// </summary>
    [MaxLength(100)]
    public required string CustomerName { get; set; }

    /// <summary>
    /// 发票条目列表
    /// </summary>
    public List<InvoiceItem> Items { get; set; } = [];

    /// <summary>
    /// 发票总金额
    /// </summary>
    public decimal TotalAmount { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public InvoiceStatus Status { get; set; }
}

/// <summary>
/// 对账单状态
/// </summary>
public enum InvoiceStatus
{
    /// <summary>
    /// 未开票
    /// </summary>
    [Description("未开票")]
    NotInvoiced,

    /// <summary>
    /// 已开票
    /// </summary>
    [Description("已开票")]
    Invoiced,

    /// <summary>
    /// 已结清
    /// </summary>
    [Description("已结清")]
    Settled,
    /// <summary>
    /// 未结清
    /// </summary>
    [Description("未结清")]
    Unsettled
}
