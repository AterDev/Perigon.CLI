namespace Entity.StudioMod;

/// <summary>
/// 属性
/// </summary>
public class ModelProperty
{
    /// <summary>
    /// 类型
    /// </summary>
    [MaxLength(100)]
    public required string Type { get; set; }

    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }

    [MaxLength(100)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// 是否是数组
    /// </summary>
    public bool IsList { get; set; }
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否为导航属性
    /// </summary>
    public bool IsNavigation { get; set; }
    public bool IsJsonIgnore { get; set; }

    /// <summary>
    /// 导航属性类名称
    /// </summary>
    [MaxLength(100)]
    public string? NavigationName { get; set; }
    public bool IsComplexType { get; set; }

    /// <summary>
    /// 导航属性的对应关系
    /// </summary>
    public bool? HasMany { get; set; }
    public bool IsEnum { get; set; }

    /// <summary>
    /// 是否包括set方法
    /// </summary>
    public bool HasSet { get; set; } = true;

    [MaxLength(100)]
    public string? AttributeText { get; set; }

    /// <summary>
    /// xml comment
    /// </summary>
    [MaxLength(500)]
    public string? CommentXml { get; set; }

    /// <summary>
    /// comment summary
    /// </summary>
    [MaxLength(200)]
    public string? CommentSummary { get; set; }

    /// <summary>
    /// 是否必须
    /// </summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// 可空?
    /// </summary>
    public bool IsNullable { get; set; }
    public int? MinLength { get; set; }
    public int? MaxLength { get; set; }
    public bool IsDecimal { get; set; }

    /// <summary>
    /// 尾缀，如#endregion
    /// </summary>
    [MaxLength(100)]
    public string? SuffixContent { get; set; }

    /// <summary>
    /// 默认值
    /// </summary>
    [MaxLength(100)]
    public string DefaultValue { get; set; } = string.Empty;

    [ForeignKey(nameof(ModelInfoId))]
    public ModelInfo EntityInfo { get; set; } = null!;
    public Guid ModelInfoId { get; set; } = default!;
}
