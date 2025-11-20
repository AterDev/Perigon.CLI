namespace CMSMod.Models.ArticleCategoryDtos;

/// <summary>
/// 目录添加时请求结构
/// </summary>
/// <inheritdoc cref="ArticleCategory"/>
public class ArticleCategoryAddDto
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}
