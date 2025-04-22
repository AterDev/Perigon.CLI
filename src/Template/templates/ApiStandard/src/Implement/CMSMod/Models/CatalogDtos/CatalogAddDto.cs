using Entity.CMSMod;

namespace CMSMod.Models.CatalogDtos;
/// <summary>
/// 目录添加时请求结构
/// </summary>
/// <inheritdoc cref="Catalog"/>
public class CatalogAddDto
{
    /// <summary>
    /// 目录名称
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}
