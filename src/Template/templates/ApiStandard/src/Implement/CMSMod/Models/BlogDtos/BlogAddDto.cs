using Entity.CMSMod;

namespace CMSMod.Models.BlogDtos;
/// <summary>
/// 博客添加时请求结构
/// </summary>
/// <inheritdoc cref="Blog"/>
public class BlogAddDto
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
    /// 标题
    /// </summary>
    [MaxLength(200)]
    public string? TranslateTitle { get; set; }
    /// <summary>
    /// 翻译内容
    /// </summary>
    [MaxLength(12000)]
    public string? TranslateContent { get; set; }
    /// <summary>
    /// 语言类型
    /// </summary>
    public LanguageType LanguageType { get; set; } = LanguageType.CN;
    /// <summary>
    /// 全站类别
    /// </summary>
    public BlogType BlogType { get; set; }
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
    public Guid CatalogId { get; set; }
    /// <summary>
    /// 浏览量
    /// </summary>
    public int ViewCount { get; set; }

}
