using Entity.OrderMod;

namespace OrderMod.Models.OrderDtos;
/// <summary>
/// 订单概要
/// </summary>
/// <see cref="Entity.OrderMod.Order"/>
public class OrderDetailDto
{
    /// <summary>
    /// 订单编号 
    /// </summary>
    [MaxLength(30)]
    public string OrderNumber { get; set; } = null!;
    /// <summary>
    /// 订单中的产品清单。
    /// </summary>
    public Product Product { get; set; } = default!;
    /// <summary>
    /// 客户
    /// </summary>
    public User User { get; set; } = default!;
    /// <summary>
    /// 订单的总价格。
    /// </summary>
    public decimal TotalPrice { get; set; }
    /// <summary>
    /// 订单当前状态。
    /// </summary>
    public OrderStatus Status { get; set; }
    public Guid Id { get; set; }

}
