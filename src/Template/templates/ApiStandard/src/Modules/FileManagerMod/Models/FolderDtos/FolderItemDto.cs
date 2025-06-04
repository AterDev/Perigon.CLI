namespace FileManagerMod.Models.FolderDtos;
/// <summary>
/// 文件夹列表元素
/// </summary>
/// <see cref="Entity.FileManagerMod.Folder"/>
public class FolderItemDto
{
    /// <summary>
    /// 名称
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = default!;
    public Guid? ParentId { get; set; }
    /// <summary>
    /// 路径
    /// </summary>
    public string? Path { get; set; }
    public Guid Id { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

}
