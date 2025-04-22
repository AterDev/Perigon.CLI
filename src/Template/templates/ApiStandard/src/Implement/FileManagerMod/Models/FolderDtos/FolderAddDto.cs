namespace FileManagerMod.Models.FolderDtos;
/// <summary>
/// 文件夹添加时请求结构
/// </summary>
/// <see cref="Entity.FileManagerMod.Folder"/>
public class FolderAddDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public required string Name { get; set; }
    public Guid? ParentId { get; set; }
}
