using Entity.OrderMod;

namespace OrderMod.Models.OrderDtos;
/// <summary>
/// 订单更新时请求结构
/// </summary>
/// <see cref="Entity.OrderMod.Order"/>
public class OrderUpdateDto
{
    /// <summary>
    /// 订单当前状态。
    /// </summary>
    public OrderStatus? Status { get; set; }
}
