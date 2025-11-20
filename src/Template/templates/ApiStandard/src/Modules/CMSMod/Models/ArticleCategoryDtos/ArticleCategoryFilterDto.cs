namespace CMSMod.Models.ArticleCategoryDtos;

/// <summary>
/// 目录查询筛选
/// </summary>
/// <inheritdoc cref="ArticleCategory"/>
public class ArticleCategoryFilterDto : FilterBase
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public string? Name { get; set; }
}
