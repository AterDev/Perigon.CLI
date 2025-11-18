namespace Entity.CMSMod;

/// <summary>
/// 内容
/// </summary>
[Index(nameof(UserId), nameof(Title), IsUnique = true)]
public class Article : EntityBase
{
    /// <summary>
    /// 标题
    /// </summary>
    [MaxLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>
    /// 内容
    /// </summary>
    [MaxLength(10000)]
    public required string Content { get; set; }

    /// <summary>
    /// 作者
    /// </summary>
    [MaxLength(200)]
    public required string Authors { get; set; }

    /// <summary>
    /// 语言类型
    /// </summary>
    public LanguageType LanguageType { get; set; } = LanguageType.CN;

    /// <summary>
    /// 全站类别
    /// </summary>
    public ContentType ContentType { get; set; }

    /// <summary>
    /// 是否审核
    /// </summary>
    public bool IsAudit { get; set; }

    /// <summary>
    /// 是否公开
    /// </summary>
    public bool IsPublic { get; set; } = true;

    /// <summary>
    /// 是否原创
    /// </summary>
    public bool IsOriginal { get; set; }

    public Guid UserId { get; set; }

    /// <summary>
    /// 所属目录
    /// </summary>
    [ForeignKey(nameof(CatalogId))]
    public ArticleCategory Catalog { get; set; } = null!;
    public Guid CatalogId { get; set; }

    /// <summary>
    /// 浏览量
    /// </summary>
    public int ViewCount { get; set; }
}

public enum ContentType
{
    /// <summary>
    /// News
    /// </summary>
    [Description("News")]
    News,

    /// <summary>
    /// ViewPoint
    /// </summary>
    [Description("ViewPoint")]
    ViewPoint,

    /// <summary>
    /// Knowledge
    /// </summary>
    [Description("Knowledge")]
    Knowledge,

    /// <summary>
    /// Documentary
    /// </summary>
    [Description("Documentary")]
    Documentary,

    /// <summary>
    /// Private
    /// </summary>
    [Description("Private")]
    Private,
}

public enum LanguageType
{
    /// <summary>
    /// zn-CN
    /// </summary>
    [Description("zh-CN")]
    CN,

    /// <summary>
    /// en-US
    /// </summary>
    [Description("en-US")]
    EN,
}
