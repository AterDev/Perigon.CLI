using Entity.OrderMod;

namespace OrderMod.Models.OrderDtos;
/// <summary>
/// 订单列表元素
/// </summary>
/// <see cref="Entity.OrderMod.Order"/>
public class OrderItemDto
{
    /// <summary>
    /// 订单编号 
    /// </summary>
    [MaxLength(30)]
    public string OrderNumber { get; set; } = default!;
    /// <summary>
    /// 订单的总价格。
    /// </summary>
    public decimal TotalPrice { get; set; }
    /// <summary>
    /// 原价格
    /// </summary>
    public decimal OriginPrice { get; set; }
    /// <summary>
    /// 订单当前状态。
    /// </summary>
    public OrderStatus Status { get; set; }
    public Guid Id { get; set; }
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

}
