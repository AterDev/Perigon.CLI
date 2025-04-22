using Entity.CMSMod;

namespace CMSMod.Models.CatalogDtos;
/// <summary>
/// 目录更新时请求结构
/// </summary>
/// <inheritdoc cref="Catalog"/>
public class CatalogUpdateDto
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public string Name { get; set; } = default!;
}
