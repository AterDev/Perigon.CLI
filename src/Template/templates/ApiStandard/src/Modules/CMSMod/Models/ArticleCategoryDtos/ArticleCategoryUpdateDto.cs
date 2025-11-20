namespace CMSMod.Models.ArticleCategoryDtos;

/// <summary>
/// 目录更新时请求结构
/// </summary>
/// <inheritdoc cref="ArticleCategory"/>
public class ArticleCategoryUpdateDto
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = default!;
}
