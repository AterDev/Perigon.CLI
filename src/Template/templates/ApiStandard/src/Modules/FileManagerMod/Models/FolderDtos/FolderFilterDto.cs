using Framework.Common.Models;

namespace FileManagerMod.Models.FolderDtos;
/// <summary>
/// 文件夹查询筛选
/// </summary>
/// <see cref="Entity.FileManagerMod.Folder"/>
public class FolderFilterDto : FilterBase
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }
}
