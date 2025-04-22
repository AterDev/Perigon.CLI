using Entity.OrderMod;
using Framework.Common.Models;

namespace OrderMod.Models.ProductDtos;
/// <summary>
/// 产品查询筛选
/// </summary>
/// <see cref="Entity.OrderMod.Product"/>
public class ProductFilterDto : FilterBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(60)]
    public string? Name { get; set; }

    /// <summary>
    /// 产品类型
    /// </summary>
    public ProductType? ProductType { get; set; }
}
