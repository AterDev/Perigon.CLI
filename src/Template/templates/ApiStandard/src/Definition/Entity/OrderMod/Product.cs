namespace Entity.OrderMod;
/// <summary>
/// 产品
/// </summary>
[Module(Modules.Order)]
[Index(nameof(Sort))]
[Index(nameof(Name))]
[Index(nameof(ProductType))]
public class Product : EntityBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(60)]
    public required string Name { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// 产品图片
    /// </summary>
    public List<string> Images { get; set; } = [];

    /// <summary>
    /// 产品视频
    /// </summary>
    public List<string> Videos { get; set; } = [];

    /// <summary>
    /// 价格
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal Price { get; set; }

    /// <summary>
    /// 积分兑换
    /// </summary>
    public int CostScore { get; set; }

    /// <summary>
    /// 排序
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// 有效期：天
    /// </summary>
    public int Days { get; set; }

    /// <summary>
    /// 产品类型
    /// </summary>
    public ProductType ProductType { get; set; }

    public ProductStatus Status { get; set; }

    /// <summary>
    /// 原价
    /// </summary>
    [Column(TypeName = "decimal(10,2)")]
    public decimal OriginPrice { get; set; }
    public ICollection<Order> Orders { get; set; } = [];
}

/// <summary>
/// 产品状态
/// </summary>
public enum ProductStatus
{
    /// <summary>
    /// 默认待上架
    /// </summary>
    [Description("默认待上架")]
    Default,
    /// <summary>
    /// 上架
    /// </summary>
    [Description("上架")]
    OnSale,
    /// <summary>
    /// 下架
    /// </summary>
    [Description("下架")]
    OffSale,
}

/// <summary>
/// 产品类型
/// </summary>
public enum ProductType
{
    /// <summary>
    /// 试用
    /// </summary>
    [Description("试用")]
    Trial,
    /// <summary>
    /// 商品
    /// </summary>
    [Description("商品")]
    Goods,
    /// <summary>
    /// 服务
    /// </summary>
    [Description("服务")]
    Service,
    /// <summary>
    /// 会员
    /// </summary>
    [Description("会员")]
    Member,
    /// <summary>
    /// 高级会员
    /// </summary>
    [Description("高级会员")]
    AdvancedMember,
}
