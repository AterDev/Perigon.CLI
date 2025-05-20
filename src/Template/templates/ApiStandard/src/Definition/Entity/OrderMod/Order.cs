using System.Runtime.CompilerServices;
using Ater.Common.Utils;
using Entity.CustomerMod;
using Entity.UserMod;

namespace Entity.OrderMod;
/// <summary>
/// 订单
/// </summary>
[Module(Modules.Order)]
[Index(nameof(OrderNumber))]
[Index(nameof(DiscountCode))]
[Index(nameof(Status))]
public class Order : EntityBase
{
    #region 关联属性
    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
    public Guid UserId { get; set; }

    [ForeignKey(nameof(CustomerInfoId))]
    public CustomerInfo? CustomerInfo { get; set; }
    public Guid CustomerInfoId { get; set; }


    #endregion

    /// <summary>
    /// 订单编号 
    /// </summary>
    [MaxLength(30)]
    public string OrderNumber { get; set; } = HashCrypto.GetRnd() + DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

    /// <summary>
    /// 支付订单号
    /// </summary>
    [MaxLength(100)]
    public string? PayNumber { get; set; }

    /// <summary>
    /// 对应产品
    /// </summary>
    [ForeignKey(nameof(ProductId))]
    public Product Product { get; set; } = null!;
    public Guid ProductId { get; set; }

    /// <summary>
    /// 产品名称
    /// </summary>
    [MaxLength(60)]
    public required string ProductName { get; set; }

    /// <summary>
    /// 原价格
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal OriginPrice { get; set; }
    /// <summary>
    /// 支付价格
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    /// <summary>
    /// 优惠码
    /// </summary>
    [MaxLength(10)]
    public string? DiscountCode { get; set; }
    /// <summary>
    /// 订单当前状态。
    /// </summary>
    public OrderStatus Status { get; set; }
}
/// <summary>
/// 订单状态
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// 未支付
    /// </summary>
    [Description("未支付")]
    UnPaid,
    /// <summary>
    /// 已取消
    /// </summary>
    [Description("已取消")]
    Cancelled,
    /// <summary>
    /// 已支付
    /// </summary>
    [Description("已支付")]
    Paid
}
