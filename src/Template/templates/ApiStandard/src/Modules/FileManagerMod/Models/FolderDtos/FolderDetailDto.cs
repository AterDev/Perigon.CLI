using Entity.FileManagerMod;

namespace FileManagerMod.Models.FolderDtos;
/// <summary>
/// 文件夹概要
/// </summary>
/// <see cref="Entity.FileManagerMod.Folder"/>
public class FolderDetailDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    public Folder? Parent { get; set; }
    public Guid? ParentId { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }
    public Guid Id { get; set; }

}
