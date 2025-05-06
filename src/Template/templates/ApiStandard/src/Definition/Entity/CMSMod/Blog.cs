using Entity.UserMod;

namespace Entity.CMSMod;
/// <summary>
/// 博客
/// </summary>
[Module(Modules.CMS)]
[LogDescription("博客", nameof(Title), Modules.CMS)]
public class Blog : EntityBase
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

    [ForeignKey(nameof(UserId))]
    public required User User { get; set; }
    public Guid UserId { get; set; }
    /// <summary>
    /// 所属目录
    /// </summary>
    [ForeignKey(nameof(CatalogId))]
    public Catalog Catalog { get; set; } = null!;
    public Guid CatalogId { get; set; }
    /// <summary>
    /// 浏览量
    /// </summary>
    public int ViewCount { get; set; }
}

public enum BlogType
{
    /// <summary>
    /// 资讯
    /// </summary>
    [Description("资讯")]
    News,
    /// <summary>
    /// 见解与分析
    /// </summary>
    [Description("见解与分析")]
    View,
    /// <summary>
    /// 教程
    /// </summary>
    [Description("教程")]
    Course,
    /// <summary>
    /// 技能分享
    /// </summary>
    [Description("技能分享")]
    Skill,
    /// <summary>
    /// 其它
    /// </summary>
    [Description("其它")]
    Else
}

public enum LanguageType
{
    /// <summary>
    /// 中文
    /// </summary>
    [Description("zh-CN")]
    CN,
    /// <summary>
    /// 英文
    /// </summary>
    [Description("en-US")]
    EN
}