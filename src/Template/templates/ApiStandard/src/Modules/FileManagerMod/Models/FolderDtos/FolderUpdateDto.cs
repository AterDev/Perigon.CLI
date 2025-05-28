namespace FileManagerMod.Models.FolderDtos;
/// <summary>
/// 文件夹更新时请求结构
/// </summary>
/// <see cref="Entity.FileManagerMod.Folder"/>
public class FolderUpdateDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public string? Name { get; set; }
    public Guid? ParentId { get; set; }

}
