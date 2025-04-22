using Entity.OrderMod;
using Framework.Common.Models;

namespace OrderMod.Models.OrderDtos;
/// <summary>
/// 订单查询筛选
/// </summary>
/// <see cref="Entity.OrderMod.Order"/>
public class OrderFilterDto : FilterBase
{
    /// <summary>
    /// 订单编号 
    /// </summary>
    [MaxLength(30)]
    public string? OrderNumber { get; set; }

    /// <summary>
    /// 订单当前状态。
    /// </summary>
    public OrderStatus? Status { get; set; }
    public Guid? ProductId { get; set; }
    public Guid? UserId { get; set; }
}
