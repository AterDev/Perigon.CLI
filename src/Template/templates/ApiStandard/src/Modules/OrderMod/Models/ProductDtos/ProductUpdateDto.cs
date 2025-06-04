using Entity.OrderMod;

namespace OrderMod.Models.ProductDtos;
/// <summary>
/// 产品更新时请求结构
/// </summary>
/// <see cref="Entity.OrderMod.Product"/>
public class ProductUpdateDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(60)]
    public string? Name { get; set; }
    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }
    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }
    /// <summary>
    /// 价格
    /// </summary>
    public decimal? Price { get; set; }
    /// <summary>
    /// 原价
    /// </summary>
    public decimal? OriginPrice { get; set; }
    /// <summary>
    /// 积分兑换
    /// </summary>
    public int? CostScore { get; set; }

    /// <summary>
    /// 有效期：天
    /// </summary>
    public int? Days { get; set; }

    /// <summary>
    /// 产品类型
    /// </summary>
    public ProductType? ProductType { get; set; }

}
