namespace OrderMod.Models.OrderDtos;
/// <summary>
/// 新订单
/// </summary>
/// <see cref="Entity.OrderMod.Order"/>
public class OrderAddDto
{
    /// <summary>
    /// 优惠码
    /// </summary>
    [MaxLength(10)]
    public string? DiscountCode { get; set; }
    public required Guid ProductId { get; set; }

}
