namespace FileManagerMod.Models.FileDataDtos;
/// <summary>
/// 文件数据列表元素
/// </summary>
/// <see cref="Entity.FileManagerMod.FileData"/>
public class FileDataItemDto
{
    /// <summary>
    /// 文件名
    /// </summary>
    [MaxLength(200)]
    public string FileName { get; set; } = default!;
    /// <summary>
    /// 扩展名
    /// </summary>
    [MaxLength(20)]
    public string? Extension { get; set; }
    /// <summary>
    /// md5值
    /// </summary>
    [MaxLength(100)]
    public string Md5 { get; set; } = default!;
    public Guid Id { get; set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedTime { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 图片内容
    /// </summary>
    public byte[]? Content { get; set; }

}
