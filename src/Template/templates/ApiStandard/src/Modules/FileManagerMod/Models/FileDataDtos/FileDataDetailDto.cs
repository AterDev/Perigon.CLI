using Entity.FileManagerMod;

namespace FileManagerMod.Models.FileDataDtos;
/// <summary>
/// 文件数据概要
/// </summary>
/// <see cref="Entity.FileManagerMod.FileData"/>
public class FileDataDetailDto
{
    public Folder? Folder { get; set; }
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

}
