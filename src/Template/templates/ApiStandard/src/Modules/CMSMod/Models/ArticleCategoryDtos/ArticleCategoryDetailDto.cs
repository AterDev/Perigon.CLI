namespace CMSMod.Models.ArticleCategoryDtos;

/// <summary>
/// 目录概要
/// </summary>
/// <inheritdoc cref="ArticleCategory"/>
public class ArticleCategoryDetailDto
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = default!;

    /// <summary>
    /// 层级
    /// </summary>
    public short Level { get; set; }

    /// <summary>
    /// 父目录
    /// </summary>
    public ArticleCategory? Parent { get; set; }
    public Guid? ParentId { get; set; }
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset UpdatedTime { get; set; } = DateTimeOffset.UtcNow;
}
